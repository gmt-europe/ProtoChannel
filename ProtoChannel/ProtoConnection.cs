using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using ProtoBuf.Meta;
using ProtoChannel.Util;
#if _NET_4
using System.Threading.Tasks;
#else
using System.Threading;
#endif
using Common.Logging;

namespace ProtoChannel
{
    internal abstract class ProtoConnection : TcpConnection, IProtoConnection
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProtoConnection));

        private static readonly byte[] FrameSpacing = new byte[4];
        private static readonly RuntimeTypeModel _typeModel = CreateTypeModel();

        private static RuntimeTypeModel CreateTypeModel()
        {
            var result = RuntimeTypeModel.Create();

            result.Add(typeof(Messages.Error), true);
            result.Add(typeof(Messages.HandshakeRequest), true);
            result.Add(typeof(Messages.HandshakeResponse), true);
            result.Add(typeof(Messages.StartStream), true);

            return result;
        }

        private PendingPackage? _pendingPackage;
        private readonly SendStreamManager _sendStreamManager;
        private readonly ReceiveStreamManager _receiveStreamManager;
        private readonly PendingMessageManager _messageManager = new PendingMessageManager();
        private readonly ServiceAssembly _serviceAssembly;
        private readonly IStreamTransferListener _streamTransferListener;
        private readonly Queue<PendingRequest> _pendingRequests = new Queue<PendingRequest>();
        private bool _disposed;

        public Client Client { get; set; }

        public ProtoCallbackChannel CallbackChannel { get; set; }

        public static RuntimeTypeModel TypeModel
        {
            get { return _typeModel; }
        }

        protected ProtoConnection(TcpClient tcpClient, IStreamManager streamManager, ServiceAssembly serviceAssembly, IStreamTransferListener streamTransferListener)
            : base(tcpClient)
        {
            Require.NotNull(streamManager, "streamManager");
            Require.NotNull(serviceAssembly, "serviceAssembly");

            _serviceAssembly = serviceAssembly;
            _streamTransferListener = streamTransferListener;
            _sendStreamManager = new SendStreamManager(streamTransferListener);
            _receiveStreamManager = new ReceiveStreamManager(streamManager, streamTransferListener);
        }

        protected override bool ProcessInput()
        {
            // Locked by TcpConnection.

            if (!_pendingPackage.HasValue)
            {
                // Is the header in the buffer?

                if (ReadAvailable < 4)
                    return false;

                byte[] buffer = new byte[4];

                buffer[0] = 0;

                Read(buffer, 0, buffer.Length);

                uint header = BitConverterEx.ToNetworkUInt32(buffer, 0);

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
                case PackageType.Pong:
                    // No-op's and pongs are just dropped.
                    break;

                case PackageType.Message:
                    ProcessMessagePackage(package);
                    break;

                case PackageType.Stream:
                    ProcessStreamPackage(package);
                    break;

                case PackageType.Ping:
                    EndSendPackage(PackageType.Pong, BeginSendPackage());
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
            _receiveStreamManager.SetError(exception);

            RaiseUnhandledException(exception);

            Dispose();
        }

        private void ProcessMessagePackage(PendingPackage package)
        {
            // We need at least three bytes for a valid message.

            if (package.Length < 4)
            {
                SendError(ProtocolError.InvalidPackageLength);
                return;
            }

            uint length = package.Length - 4;

            byte[] buffer = new byte[4];

            buffer[0] = 0;

            Read(buffer, 0, buffer.Length);

            uint header = BitConverterEx.ToNetworkUInt32(buffer, 0);

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

                associationId = BitConverterEx.ToNetworkUInt16(buffer, 0);
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

            if (!Client.ServiceAssembly.MessagesById.TryGetValue((int)type, out messageType))
            {
                SendError(ProtocolError.InvalidMessageType);
                return;
            }

            // Parse the message.

            object message = ReadMessage(
                Client.ServiceAssembly.TypeModel, messageType.Type, (int)length
            );

            // Dispatch the message.

            var dispatcher = Client.Instance as IProtoMessageDispatcher;

            if (dispatcher != null)
            {
                if (isOneWay)
                {
                    dispatcher.DispatchPost(message);
                }
                else
                {
                    var dispatchMessage = new DispatchMessage(dispatcher, associationId, this);

                    dispatcher.BeginDispatch(message, dispatchMessage.BeginDispatchCallback, null);
                }

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

            // Start processing the message.

            var pendingRequest = new PendingRequest(message, isOneWay, associationId, method);

            _pendingRequests.Enqueue(pendingRequest);

            if (_pendingRequests.Count == 1)
            {
#if _NET_2 || _NET_MD
                ThreadPool.QueueUserWorkItem(ExecuteRequests, pendingRequest);
#else
                Task.Factory.StartNew(ExecuteRequests, pendingRequest);
#endif
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void ExecuteRequests(object arg)
        {
            var pendingRequest = (PendingRequest)arg;

            try
            {
                while (true)
                {
                    object result;

                    var client = Client;

                    if (client != null)
                    {
                        lock (client.SyncRoot)
                        {
                            if (!client.IsDisposed)
                            {
                                using (OperationContext.SetScope(new OperationContext(this, CallbackChannel)))
                                {
                                    result = pendingRequest.Method.Invoke(
                                        client.Instance, pendingRequest.Message
                                    );
                                }
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                    else
                    {
                        return;
                    }

                    lock (SyncRoot)
                    {
                        if (IsDisposed)
                            return;

                        _pendingRequests.Dequeue();

                        if (!pendingRequest.IsOneWay)
                            SendResponse(result, pendingRequest.Method.Response.Id, pendingRequest.AssociationId);

                        if (_pendingRequests.Count == 0)
                            break;

                        pendingRequest = _pendingRequests.Peek();
                    }
                }
            }
            catch (Exception ex)
            {
                SendError(ProtocolError.ErrorProcessingRequest);

                RaiseUnhandledException(ex);
            }
        }

        protected abstract void RaiseUnhandledException(Exception exception);

        private void SendResponse(object result, int messageType, uint associationId)
        {
            long packageStart = BeginSendPackage();

            // Write the header.

            uint header = (uint)MessageKind.Response | (uint)messageType << 2;

            byte[] buffer = BitConverterEx.GetNetworkBytes(header);

            Write(buffer, 0, buffer.Length);

            // Write the association ID.

            buffer = BitConverterEx.GetNetworkBytes((ushort)associationId);

            Write(buffer, 0, buffer.Length);

            // Write the message.

            WriteMessage(Client.ServiceAssembly.TypeModel, result);

            EndSendPackage(PackageType.Message, packageStart);
        }

        private void ProcessResponseMessage(uint type, uint length, uint associationId)
        {
            var pendingMessage = _messageManager.RemovePendingMessage(associationId);

            if (pendingMessage.MessageType != null && type != pendingMessage.MessageType.Id)
            {
                pendingMessage.SetAsCompleted(
                    new ProtoChannelException("Response was of an unexpected message type"), false
                );

                SendError(ProtocolError.UnexpectedMessageType);
                return;
            }

            var messageType = pendingMessage.MessageType ?? _serviceAssembly.MessagesById[(int)type];

            object message = ReadMessage(
                _serviceAssembly.TypeModel, messageType.Type, (int)length
            );

            pendingMessage.SetAsCompleted(message, false);
        }

        private void ProcessStreamPackage(PendingPackage package)
        {
            if (package.Length < 4)
            {
                SendError(ProtocolError.InvalidPackageLength);
                return;
            }

            // Parse the header.

            byte[] buffer = new byte[4];

            buffer[0] = 0;

            Read(buffer, 0, buffer.Length);

            uint header = BitConverterEx.ToNetworkUInt32(buffer, 0);

            // Get the details from the header.

            uint streamPackageTypeNumber = header & 0x7;
            int associationId = (int)(header >> 3);

            // Process the stream package.

            switch ((StreamPackageType)streamPackageTypeNumber)
            {
                case StreamPackageType.StartStream:
                    ProcessStartStreamPackage(associationId, (int)package.Length - 4);
                    break;

                case StreamPackageType.AcceptStream:
                    ProcessAcceptStreamPackage(associationId);
                    break;

                case StreamPackageType.RejectStream:
                    ProcessRejectStreamPackage(associationId);
                    break;

                case StreamPackageType.StreamData:
                    ProcessStreamDataPackage(associationId, (int)package.Length - 4);
                    break;

                case StreamPackageType.EndStream:
                case StreamPackageType.StreamFailed:
                    ProcessEndStreamPackage(associationId, (StreamPackageType)streamPackageTypeNumber != StreamPackageType.StreamFailed);
                    break;

                default:
                    SendError(ProtocolError.InvalidStreamPackageType);
                    break;
            }
        }

        private void ProcessStartStreamPackage(int associationId, int packageLength)
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

            uint header = (uint)responseType | (uint)associationId << 3;

            var buffer = BitConverterEx.GetNetworkBytes(header);

            // Write the header.

            Write(buffer, 0, buffer.Length);

            // And send the package.

            EndSendPackage(PackageType.Stream, packageStart);
        }

        private void ProcessAcceptStreamPackage(int associationId)
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

        private void ProcessRejectStreamPackage(int associationId)
        {
            var error = _sendStreamManager.RejectStream(associationId);

            if (error.HasValue)
                SendError(error.Value);
        }

        private void ProcessStreamDataPackage(int associationId, int length)
        {
            PendingReceiveStream stream;

            var error = _receiveStreamManager.TryGetStream(associationId, out stream);

            if (error.HasValue)
            {
                SendError(error.Value);
                return;
            }

            ReadStream(stream.Stream, length);

            RaiseEvent(stream, StreamTransferEventType.Transfer);
        }

        private void ProcessEndStreamPackage(int associationId, bool success)
        {
            var error = _receiveStreamManager.EndStream(associationId, success);

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

            long messageLength = position - packageStart - 4;

            uint messageHeader = (uint)messageLength << 3 | (uint)packageType;

            var buffer = BitConverterEx.GetNetworkBytes(messageHeader);

            Write(buffer, 0, buffer.Length);

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
                    SendStreamEnd(sendRequest.Value, true);
                else
                    SendStreamData(sendRequest.Value);
            }
        }

        private void SendStreamEnd(StreamSendRequest request, bool success)
        {
            _sendStreamManager.RemoveStream(request.Stream);

            long packageStart = BeginSendPackage();

            WriteStreamPackageHeader(request, success ? StreamPackageType.EndStream : StreamPackageType.StreamFailed);

            EndSendPackage(PackageType.Stream, packageStart, false);

            RaiseEvent(request.Stream, StreamTransferEventType.End);
        }

        private void SendStreamData(StreamSendRequest request)
        {
            // Send the stream data package.

            long packageStart = BeginSendPackage();

            WriteStreamPackageHeader(request, StreamPackageType.StreamData);

            // Write the stream data. If this fails, we send a stream
            // failure instead.

            try
            {
                WriteStream(request.Stream.Stream, request.Length);

                RaiseEvent(request.Stream, StreamTransferEventType.Transfer);
            }
            catch (Exception ex)
            {
                // If the send stream failed, back up and send the stream
                // failed package.

                Log.Warn("Exception while reading from stream", ex);

                WriteLength = WritePosition = packageStart;

                SendStreamEnd(request, false);

                return;
            }

            request.Stream.Position += request.Length;

            // And send the package.

            EndSendPackage(PackageType.Stream, packageStart, false);
        }

        private void WriteStreamPackageHeader(StreamSendRequest request, StreamPackageType streamPackageType)
        {
            uint header = (uint)streamPackageType | (uint)request.Stream.AssociationId << 3;

            var buffer = BitConverterEx.GetNetworkBytes(header);

            Write(buffer, 0, buffer.Length);
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

        public int SendStream(Stream stream, string streamName, string contentType)
        {
            return SendStream(stream, streamName, contentType, StreamDisposition.Attachment);
        }

        public int SendStream(Stream stream, string streamName, string contentType, StreamDisposition disposition)
        {
            return SendStream(stream, streamName, contentType, disposition, null);
        }

        public int SendStream(Stream stream, string streamName, string contentType, int? associationId)
        {
            return SendStream(stream, streamName, contentType, StreamDisposition.Attachment, associationId);
        }

        public int SendStream(Stream stream, string streamName, string contentType, StreamDisposition disposition, int? associationId)
        {
            lock (SyncRoot)
            {
                associationId = _sendStreamManager.RegisterStream(
                    stream, streamName, contentType, disposition, associationId
                );

                // Send the start of the stream.

                long packageStart = BeginSendPackage();

                // Construct the header.

                uint header = (uint)StreamPackageType.StartStream | (uint)associationId.Value << 3;

                var buffer = BitConverterEx.GetNetworkBytes(header);

                Write(buffer, 0, buffer.Length);

                // Write the details of the request.

                WriteMessage(TypeModel, new Messages.StartStream
                {
                    Length = (uint)stream.Length,
                    StreamName = streamName,
                    ContentType = contentType,
                    Attachment = disposition == StreamDisposition.Attachment
                });

                // Send the package.

                EndSendPackage(PackageType.Stream, packageStart);

                return associationId.Value;
            }
        }

        public ProtoStream GetStream(int streamId)
        {
            return EndGetStream(BeginGetStream(streamId, null, null));
        }

        public IAsyncResult BeginGetStream(int streamId, AsyncCallback callback, object asyncState)
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
            Require.NotNull(message, "message");

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

                var buffer = BitConverterEx.GetNetworkBytes(header);

                Write(buffer, 0, buffer.Length);

                // Write the association ID.

                var pendingMessage = _messageManager.GetPendingMessage(responseMessageType, callback, asyncState);

                buffer = BitConverterEx.GetNetworkBytes((ushort)pendingMessage.AssociationId);

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
            Require.NotNull(asyncResult, "asyncResult");

            var result = ((PendingMessage)asyncResult).EndInvoke();

            if (result is Exception)
                throw (Exception)result;

            return result;
        }

        public void PostMessage(object message)
        {
            Require.NotNull(message, "message");

            lock (SyncRoot)
            {
                ServiceMessage messageType;

                if (!_serviceAssembly.MessagesByType.TryGetValue(message.GetType(), out messageType))
                    throw new ProtoChannelException(String.Format("Message type '{0}' is not a valid message type", message.GetType()));

                long packageStart = BeginSendPackage();

                // Write the header.

                uint header = (uint)MessageKind.OneWay | (uint)messageType.Id << 2;

                var buffer = BitConverterEx.GetNetworkBytes(header);

                Write(buffer, 0, buffer.Length);

                // Write the message.

                WriteMessage(_serviceAssembly.TypeModel, message);

                // Send the message.

                EndSendPackage(PackageType.Message, packageStart);
            }
        }

        private void RaiseEvent(PendingStream pendingStream, StreamTransferEventType eventType)
        {
            if (_streamTransferListener != null)
                _streamTransferListener.RaiseStreamTransfer(pendingStream, eventType);
        }

        protected override void Dispose(bool disposing)
        {
            lock (SyncRoot)
            {
                if (!_disposed && disposing)
                {
                    // Cancel all outstanding requests.

                    var disconnectException = new ProtoChannelException("Channel disconnected");

                    _messageManager.SetError(disconnectException);
                    _receiveStreamManager.SetError(disconnectException);

                    if (Client != null)
                    {
                        Client.Dispose();
                        Client = null;
                    }

                    _disposed = true;
                }
            }

            base.Dispose(disposing);
        }

        private class DispatchMessage
        {
            private readonly IProtoMessageDispatcher _dispatcher;
            private readonly uint _associationId;
            private readonly ProtoConnection _connection;

            public DispatchMessage(IProtoMessageDispatcher dispatcher, uint associationId, ProtoConnection connection)
            {
                Require.NotNull(dispatcher, "dispatcher");
                Require.NotNull(connection, "connection");

                _dispatcher = dispatcher;
                _associationId = associationId;
                _connection = connection;
            }

            public void BeginDispatchCallback(IAsyncResult asyncResult)
            {
                lock (_connection.SyncRoot)
                {
                    if (_connection.IsDisposed)
                        return;

                    var response = _dispatcher.EndDispatch(asyncResult);

                    var responseMessage = _connection._serviceAssembly.MessagesByType[response.GetType()];

                    _connection.SendResponse(response, responseMessage.Id, _associationId);
                }
            }
        }
    }
}
