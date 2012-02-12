using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using ProtoBuf.Meta;
using ProtoChannel.Util;
using System.Threading.Tasks;

namespace ProtoChannel
{
    internal abstract class ProtoConnection : TcpConnection, IProtoConnection
    {
        private static readonly byte[] FrameSpacing = new byte[3];

        private PendingPackage? _pendingPackage;
        private readonly SendStreamManager _sendStreamManager;
        private readonly ReceiveStreamManager _receiveStreamManager;
        private readonly PendingMessageManager _messageManager = new PendingMessageManager();
        private readonly ServiceAssembly _serviceAssembly;
        private readonly Queue<PendingRequest> _pendingRequests = new Queue<PendingRequest>();

        public Client Client { get; set; }

        public ProtoCallbackChannel CallbackChannel { get; set; }

        public static RuntimeTypeModel TypeModel { get; private set; }

        static ProtoConnection()
        {
            TypeModel = RuntimeTypeModel.Create();

            TypeModel.Add(typeof(Messages.Error), true);
            TypeModel.Add(typeof(Messages.HandshakeRequest), true);
            TypeModel.Add(typeof(Messages.HandshakeResponse), true);
            TypeModel.Add(typeof(Messages.StartStream), true);
        }

        protected ProtoConnection(TcpClient tcpClient, IStreamManager streamManager, ServiceAssembly serviceAssembly)
            : base(tcpClient)
        {
            if (streamManager == null)
                throw new ArgumentNullException("streamManager");
            if (serviceAssembly == null)
                throw new ArgumentNullException("serviceAssembly");

            _serviceAssembly = serviceAssembly;
            _sendStreamManager = new SendStreamManager();
            _receiveStreamManager = new ReceiveStreamManager(streamManager);
        }

        protected override bool ProcessInput()
        {
            // Locked by TcpConnection.

            if (!_pendingPackage.HasValue)
            {
                // Is the header in the buffer?

                if (ReadAvailable < 3)
                    return false;

                byte[] buffer = new byte[4];

                buffer[0] = 0;

                Read(buffer, 1, buffer.Length - 1);

                ByteUtil.ConvertNetwork(buffer);

                uint header = BitConverter.ToUInt32(buffer, 0);

                // Get the information from the header.

                _pendingPackage = new PendingPackage((PackageType)(header & 0x7), header >> 3);
            }

            if (ReadAvailable < _pendingPackage.Value.Length)
                return false;

            var pendingPackage = _pendingPackage.Value;

            _pendingPackage = null;

            ProcessPackage(pendingPackage);

            return !IsDisposed && ReadAvailable > 0;
        }

        protected virtual void ProcessPackage(PendingPackage package)
        {
            switch (package.Type)
            {
                case PackageType.Error:
                    ProcessErrorPackage(package);
                    break;

                case PackageType.NoOp:
                    // No-op's are just dropped.
                    break;

                case PackageType.Message:
                    ProcessMessagePackage(package);
                    break;

                case PackageType.Stream:
                    ProcessStreamPackage(package);
                    break;

                default:
                    throw new NotSupportedException("Invalid package type");
            }
        }

        private void ProcessErrorPackage(PendingPackage package)
        {
            var error = (Messages.Error)ReadMessage(
                TypeModel, typeof(Messages.Error), (int)package.Length
            );

            var exception = new ProtoChannelException((ProtocolError)error.ErrorNumber);

            _messageManager.SetError(exception);

            RaiseUnhandledException(exception);

            Dispose();
        }

        private void ProcessMessagePackage(PendingPackage package)
        {
            // We need at least three bytes for a valid message.

            if (package.Length < 3)
            {
                SendError(ProtocolError.InvalidPackageLength);
                return;
            }

            uint length = package.Length - 3;

            byte[] buffer = new byte[4];

            buffer[0] = 0;

            Read(buffer, 1, buffer.Length - 1);

            ByteUtil.ConvertNetwork(buffer);

            uint header = BitConverter.ToUInt32(buffer, 0);

            uint messageKindNumber = header & 0x3;
            uint messageType = header >> 2;

            // Validate the message kind.

            if (messageKindNumber == 3)
            {
                SendError(ProtocolError.InvalidMessageKind);
                return;
            }

            var messageKind = (MessageKind)messageKindNumber;
            uint associationId = 0;

            if (messageKind != MessageKind.OneWay)
            {
                // Verify that there is an association ID in the request.

                if (package.Length < 5)
                {
                    SendError(ProtocolError.InvalidPackageLength);
                    return;
                }

                length -= 2;

                Read(buffer, 0, 2);

                ByteUtil.ConvertNetwork(buffer, 0, 2);

                associationId = BitConverter.ToUInt16(buffer, 0);
            }

            ProcessMessage(messageKind, messageType, length, associationId);
        }

        private void ProcessMessage(MessageKind kind, uint type, uint length, uint associationId)
        {
            if (kind == MessageKind.Response)
                ProcessResponseMessage(type, length, associationId);
            else
                ProcessRequestMessage(type, length, associationId, kind == MessageKind.OneWay);
        }

        private void ProcessRequestMessage(uint type, uint length, uint associationId, bool isOneWay)
        {
            if (Client == null)
            {
                SendError(ProtocolError.CannotProcessRequestMessage);
                return;
            }

            // Validate the request and find the method.

            ServiceMessage messageType;

            if (!Client.Service.Messages.TryGetValue((int)type, out messageType))
            {
                SendError(ProtocolError.InvalidMessageType);
                return;
            }

            ServiceMethod method;

            if (!Client.Service.Methods.TryGetValue(messageType, out method))
            {
                SendError(ProtocolError.InvalidMessageType);
                return;
            }

            if (method.IsOneWay != isOneWay)
            {
                SendError(method.IsOneWay ? ProtocolError.ExpectedIsOneWay : ProtocolError.ExpectedRequest);
                return;
            }

            // Parse the message.

            object message = ReadMessage(
                Client.Service.ServiceAssembly.TypeModel, messageType.Type, (int)length
            );

            // Start processing the message.

            lock (SyncRoot)
            {
                _pendingRequests.Enqueue(new PendingRequest(message, isOneWay, associationId, method));

                if (_pendingRequests.Count == 1)
                {
#if _NET_2
                    ThreadPool.QueueUserWorkItem(ExecuteMessages, null);
#else
                    Task.Factory.StartNew(ExecuteRequests, null);
#endif
                }
            }
        }

        private void ExecuteRequests(object unused)
        {
            try
            {
                while (true)
                {
                    PendingRequest pendingRequest;

                    lock (SyncRoot)
                    {
                        if (IsDisposed || _pendingRequests.Count == 0)
                            return;

                        // Leave the message in the queue to not trigger a new
                        // ExecuteRequests.

                        pendingRequest = _pendingRequests.Peek();
                    }

                    object result;

                    lock (Client.SyncRoot)
                    {
                        using (OperationContext.SetScope(new OperationContext(this, CallbackChannel)))
                        {
                            result = pendingRequest.Method.Method.Invoke(
                                Client.Instance, new[] { pendingRequest.Message }
                            );
                        }
                    }

                    lock (SyncRoot)
                    {
                        if (IsDisposed)
                            return;

                        _pendingRequests.Dequeue();

                        if (!pendingRequest.IsOneWay)
                            SendResponse(pendingRequest, result);
                    }
                }
            }
            catch (Exception ex)
            {
                RaiseUnhandledException(ex);
            }
        }

        protected virtual void RaiseUnhandledException(Exception exception)
        {
        }

        private void SendResponse(PendingRequest message, object result)
        {
            long packageStart = BeginSendPackage();

            // Write the header.

            uint header = (uint)MessageKind.Response | (uint)message.Method.Response.Id << 2;

            byte[] buffer = BitConverter.GetBytes(header);

            ByteUtil.ConvertNetwork(buffer);

            Write(buffer, 1, buffer.Length - 1);

            // Write the association ID.

            buffer = BitConverter.GetBytes((ushort)message.AssociationId);

            ByteUtil.ConvertNetwork(buffer);

            Write(buffer, 0, buffer.Length);

            // Write the message.

            WriteMessage(Client.Service.ServiceAssembly.TypeModel, result);

            EndSendPackage(PackageType.Message, packageStart);
        }

        private void ProcessResponseMessage(uint type, uint length, uint associationId)
        {
            var pendingMessage = _messageManager.RemovePendingMessage(associationId);

            if (type != pendingMessage.MessageType.Id)
            {
                pendingMessage.SetAsFailed(
                    new ProtoChannelException("Response was of an unexpected message type"), false
                );

                SendError(ProtocolError.UnexpectedMessageType);
                return;
            }

            object message = ReadMessage(
                _serviceAssembly.TypeModel, pendingMessage.MessageType.Type, (int)length
            );

            pendingMessage.SetAsCompleted(message, false);
        }

        private void ProcessStreamPackage(PendingPackage package)
        {
            if (package.Length < 3)
            {
                SendError(ProtocolError.InvalidPackageLength);
                return;
            }

            // Parse the header.

            byte[] buffer = new byte[4];

            buffer[0] = 0;

            Read(buffer, 1, buffer.Length - 1);

            ByteUtil.ConvertNetwork(buffer);

            uint header = BitConverter.ToUInt32(buffer, 0);

            // Get the details from the header.

            uint streamPackageTypeNumber = header & 0x7;
            uint associationId = header >> 3;

            if (streamPackageTypeNumber > 4)
            {
                SendError(ProtocolError.InvalidStreamPackageType);
                return;
            }

            // Process the stream package.

            switch ((StreamPackageType)streamPackageTypeNumber)
            {
                case StreamPackageType.StartStream:
                    ProcessStartStreamPackage(associationId, (int)package.Length - 3);
                    break;

                case StreamPackageType.AcceptStream:
                    ProcessAcceptStreamPackage(associationId);
                    break;

                case StreamPackageType.RejectStream:
                    ProcessRejectStreamPackage(associationId);
                    break;

                case StreamPackageType.StreamData:
                    ProcessStreamDataPackage(associationId, (int)package.Length - 3);
                    break;

                case StreamPackageType.EndStream:
                    ProcessEndStreamPackage(associationId);
                    break;
            }
        }

        private void ProcessStartStreamPackage(uint associationId, int packageLength)
        {
            // Get the stream start message.

            var message = (Messages.StartStream)ReadMessage(
                TypeModel, typeof(Messages.StartStream), packageLength
            );

            // Register the stream with the stream manager.

            bool success = _receiveStreamManager.RegisterStream(associationId, message);

            // Push the response.

            var responseType = success ? StreamPackageType.AcceptStream : StreamPackageType.RejectStream;

            long packageStart = BeginSendPackage();

            // Construct the header.

            uint header = (uint)responseType | associationId << 3;

            var buffer = BitConverter.GetBytes(header);

            ByteUtil.ConvertNetwork(buffer);

            // Write the header.

            Write(buffer, 1, buffer.Length - 1);

            // And send the package.

            EndSendPackage(PackageType.Stream, packageStart);
        }

        private void ProcessAcceptStreamPackage(uint associationId)
        {
            // Accepting the stream will put it into the queue for sending.
            // We kick of an async send so we actually start sending the
            // stream data.

            var error = _sendStreamManager.AcceptStream(associationId);

            if (error.HasValue)
                SendError(error.Value);
            else
                Send();
        }

        private void ProcessRejectStreamPackage(uint associationId)
        {
            var error = _sendStreamManager.RejectStream(associationId);

            if (error.HasValue)
                SendError(error.Value);
        }

        private void ProcessStreamDataPackage(uint associationId, int length)
        {
            PendingReceiveStream stream;

            var error = _receiveStreamManager.TryGetStream(associationId, out stream);

            if (error.HasValue)
            {
                SendError(error.Value);
                return;
            }

            ReadStream(stream.Stream, length);
        }

        private void ProcessEndStreamPackage(uint associationId)
        {
            var error = _receiveStreamManager.EndStream(associationId);

            if (error.HasValue)
                SendError(error.Value);
        }

        protected long BeginSendPackage()
        {
            long packageStart = WritePosition;

            // Make room for the frame header.

            Write(FrameSpacing, 0, FrameSpacing.Length);

            return packageStart;
        }

        protected void EndSendPackage(PackageType packageType, long packageStart)
        {
            EndSendPackage(packageType, packageStart, true);
        }

        protected void EndSendPackage(PackageType packageType, long packageStart, bool forceSend)
        {
            long position = WritePosition;

            WritePosition = packageStart;

            long messageLength = position - packageStart - 3;

            uint messageHeader = (uint)messageLength << 3 | (uint)packageType;

            var buffer = BitConverter.GetBytes(messageHeader);

            ByteUtil.ConvertNetwork(buffer);

            Write(buffer, 1, buffer.Length - 1);

            WritePosition = position;

            if (forceSend)
                Send();
        }

        protected override void BeforeSend()
        {
            // Locked by TcpConnection.

            // If there isn't any data pending, see whether we can send
            // stream data.

            var sendRequest = _sendStreamManager.GetSendRequest();

            // If there is nothing to send, we can quit now.

            if (sendRequest.HasValue)
            {
                if (sendRequest.Value.IsCompleted)
                    SendStreamEnd(sendRequest.Value);
                else
                    SendStreamData(sendRequest.Value);
            }
        }

        private void SendStreamEnd(StreamSendRequest request)
        {
            long packageStart = BeginSendPackage();

            WriteStreamPackageHeader(request, StreamPackageType.EndStream);

            EndSendPackage(PackageType.Stream, packageStart, false);
        }

        private void SendStreamData(StreamSendRequest request)
        {
            // Send the stream data package.

            long packageStart = BeginSendPackage();

            WriteStreamPackageHeader(request, StreamPackageType.StreamData);

            // Write the stream data.

            WriteStream(request.Stream.Stream, request.Length);

            // And send the package.

            EndSendPackage(PackageType.Stream, packageStart, false);
        }

        private void WriteStreamPackageHeader(StreamSendRequest request, StreamPackageType streamPackageType)
        {
            uint header = (uint)streamPackageType | request.Stream.AssociationId << 3;

            var buffer = BitConverter.GetBytes(header);

            ByteUtil.ConvertNetwork(buffer);

            Write(buffer, 1, buffer.Length - 1);
        }

        protected void SendError(ProtocolError error)
        {
            long packageStart = BeginSendPackage();

            WriteMessage(TypeModel, new Messages.Error
            {
                ErrorNumber = (uint)error
            });

            EndSendPackage(PackageType.Error, packageStart);
        }

        public uint SendStream(Stream stream, string streamName, string contentType)
        {
            lock (SyncRoot)
            {
                var associationId = _sendStreamManager.RegisterStream(
                    stream, streamName, contentType
                );

                // Send the start of the stream.

                long packageStart = BeginSendPackage();

                // Construct the header.

                uint header = (uint)StreamPackageType.StartStream | associationId << 3;

                var buffer = BitConverter.GetBytes(header);

                ByteUtil.ConvertNetwork(buffer);

                Write(buffer, 1, buffer.Length - 1);

                // Write the details of the request.

                WriteMessage(TypeModel, new Messages.StartStream
                {
                    Length = (uint)stream.Length,
                    StreamName = streamName,
                    ContentType = contentType
                });

                // Send the package.

                EndSendPackage(PackageType.Stream, packageStart);

                return associationId;
            }
        }

        public IAsyncResult BeginGetStream(uint streamId, AsyncCallback callback, object asyncState)
        {
            lock (SyncRoot)
            {
                return _receiveStreamManager.BeginGetStream(streamId, callback, asyncState);
            }
        }

        public ProtoStream EndGetStream(IAsyncResult asyncResult)
        {
            return PendingReceiveStream.EndGetStream(asyncResult);
        }

        public IAsyncResult BeginSendMessage(object message, Type responseType, AsyncCallback callback, object asyncState)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            lock (SyncRoot)
            {
                ServiceMessage messageType;

                if (!_serviceAssembly.MessagesByType.TryGetValue(message.GetType(), out messageType))
                    throw new ProtoChannelException(String.Format("Message type '{0}' is not a valid message type", message.GetType()));

                ServiceMessage responseMessageType = null;

                if (responseType != null)
                {
                    if (!_serviceAssembly.MessagesByType.TryGetValue(responseType, out responseMessageType))
                        throw new ProtoChannelException(String.Format("Message type '{0}' is not a valid message type", responseMessageType));
                }

                long packageStart = BeginSendPackage();

                // Write the header.

                uint header = (uint)MessageKind.Request | (uint)messageType.Id << 2;

                var buffer = BitConverter.GetBytes(header);

                ByteUtil.ConvertNetwork(buffer);

                Write(buffer, 1, buffer.Length - 1);

                // Write the association ID.

                var pendingMessage = _messageManager.GetPendingMessage(responseMessageType, callback, asyncState);

                buffer = BitConverter.GetBytes((ushort)pendingMessage.AssociationId);

                ByteUtil.ConvertNetwork(buffer);

                Write(buffer, 0, buffer.Length);

                // Write the message.

                WriteMessage(_serviceAssembly.TypeModel, message);

                // Send the message.

                EndSendPackage(PackageType.Message, packageStart);

                return pendingMessage;
            }
        }

        public object EndSendMessage(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
                throw new ArgumentNullException("asyncResult");

            return ((PendingMessage)asyncResult).EndInvoke();
        }

        public void PostMessage(object message)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            lock (SyncRoot)
            {
                ServiceMessage messageType;

                if (!_serviceAssembly.MessagesByType.TryGetValue(message.GetType(), out messageType))
                    throw new ProtoChannelException(String.Format("Message type '{0}' is not a valid message type", message.GetType()));

                long packageStart = BeginSendPackage();

                // Write the header.

                uint header = (uint)MessageKind.OneWay | (uint)messageType.Id << 2;

                var buffer = BitConverter.GetBytes(header);

                ByteUtil.ConvertNetwork(buffer);

                Write(buffer, 1, buffer.Length - 1);

                // Write the message.

                WriteMessage(_serviceAssembly.TypeModel, message);

                // Send the message.

                EndSendPackage(PackageType.Message, packageStart);
            }
        }
    }
}
