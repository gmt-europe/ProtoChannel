using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using ProtoBuf.Meta;
using ProtoChannel.Util;

namespace ProtoChannel
{
    internal abstract class ProtoConnection : IProtoConnection, IDisposable
    {
        private readonly object _syncRoot = new object();
        private PendingPackage? _pendingPackage;
        private bool _sending;
        private readonly SendStreamManager _sendStreamManager;
        private readonly ReceiveStreamManager _receiveStreamManager;

        public bool IsDisposed { get; private set; }

        protected bool IsAsync { get; set; }

        protected RingMemoryStream ReceiveBuffer { get; private set; }

        protected RingMemoryStream SendBuffer { get; private set; }

        protected Stream Stream { get; set; }

        protected TcpClient TcpClient { get; private set; }

        public static RuntimeTypeModel TypeModel { get; private set; }

        static ProtoConnection()
        {
            TypeModel = RuntimeTypeModel.Create();

            TypeModel.Add(typeof(Messages.Error), true);
            TypeModel.Add(typeof(Messages.HandshakeRequest), true);
            TypeModel.Add(typeof(Messages.HandshakeResponse), true);
            TypeModel.Add(typeof(Messages.StartStream), true);
        }

        protected ProtoConnection(TcpClient tcpClient, IStreamManager streamManager)
        {
            if (tcpClient == null)
                throw new ArgumentNullException("tcpClient");
            if (streamManager == null)
                throw new ArgumentNullException("streamManager");

            _sendStreamManager = new SendStreamManager();
            _receiveStreamManager = new ReceiveStreamManager(streamManager);

            SendBuffer = new RingMemoryStream(Constants.RingBufferBlockSize);
            ReceiveBuffer = new RingMemoryStream(Constants.RingBufferBlockSize);

            TcpClient = tcpClient;

            TcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
        }

        protected void Read()
        {
            if (IsAsync)
                ReadAsync();
            else
                ReadSync();
        }

        private void ReadSync()
        {
            lock (_syncRoot)
            {
                VerifyNotDisposed();

                var page = ReceiveBuffer.GetWriteBuffer();

                ProcessReceivedData(
                    Stream.Read(page.Buffer, page.Offset, page.Count)
                );
            }
        }

        private void ReadAsync()
        {
            if (!IsDisposed)
            {
                var page = ReceiveBuffer.GetWriteBuffer();

                Stream.BeginRead(page.Buffer, page.Offset, page.Count, ReadCallback, null);
            }
        }

        private void ReadCallback(IAsyncResult asyncResult)
        {
            lock (_syncRoot)
            {
                if (IsDisposed)
                    return;

                // Add the read bytes to the input buffer.

                try
                {
                    ProcessReceivedData(Stream.EndRead(asyncResult));

                    ReadAsync();
                }
                catch
                {
                    Dispose();
                }
            }
        }

        private void ProcessReceivedData(int read)
        {
            // Zero input means that the remote socket closed the connection.

            if (read == 0)
            {
                Dispose();
                return;
            }

            ReceiveBuffer.SetLength(ReceiveBuffer.Length + read);

            // Process all messages currently in the input buffer.

            ReceiveBuffer.Position = ReceiveBuffer.Head;

            while (!IsDisposed)
            {
                if (!ProcessInput())
                    break;
            }

            if (IsDisposed)
                return;

            // Set the position to the end so new data is appended to the end
            // of the buffer, and update the head to free unused pages.

            ReceiveBuffer.Head = ReceiveBuffer.Position;
            ReceiveBuffer.Position = ReceiveBuffer.Length;
        }

        protected virtual bool ProcessInput()
        {
            if (!_pendingPackage.HasValue)
            {
                // Is the header in the buffer?

                if (ReceiveBuffer.Length - ReceiveBuffer.Position < 3)
                    return false;

                byte[] buffer = new byte[4];

                buffer[0] = 0;

                ReceiveBuffer.Read(buffer, 1, buffer.Length - 1);

                ByteUtil.ConvertNetwork(buffer);

                uint header = BitConverter.ToUInt32(buffer, 0);

                // Get the information from the header.

                _pendingPackage = new PendingPackage((PackageType)(header & 0x7), header >> 3);
            }

            if (ReceiveBuffer.Length - ReceiveBuffer.Position < _pendingPackage.Value.Length)
                return false;

            var pendingPackage = _pendingPackage.Value;

            _pendingPackage = null;

            ProcessPackage(pendingPackage);

            return ReceiveBuffer.Length > ReceiveBuffer.Position;
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
                    throw new NotImplementedException();
            }
        }

        private void ProcessErrorPackage(PendingPackage package)
        {
            var error = (Messages.Error)TypeModel.Deserialize(
                ReceiveBuffer, null, typeof(Messages.Error), (int)package.Length
            );

            throw new ProtoChannelException(String.Format("Protocol exception '{0}'", (ProtocolError)error.ErrorNumber));
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

            ReceiveBuffer.Read(buffer, 1, buffer.Length - 1);

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

                ReceiveBuffer.Read(buffer, 0, 2);

                ByteUtil.ConvertNetwork(buffer, 0, 2);

                associationId = BitConverter.ToUInt16(buffer, 0);
            }

            ProcessMessage(messageKind, messageType, length, associationId);
        }

        protected abstract void ProcessMessage(MessageKind kind, uint type, uint length, uint associationId);

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

            ReceiveBuffer.Read(buffer, 1, buffer.Length - 1);

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

            var message = (Messages.StartStream)TypeModel.Deserialize(
                ReceiveBuffer, null, typeof(Messages.StartStream), packageLength
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

            SendBuffer.Write(buffer, 1, buffer.Length - 1);

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
            var error = _receiveStreamManager.ProcessData(associationId, ReceiveBuffer, length);

            if (error.HasValue)
                SendError(error.Value);
        }

        private void ProcessEndStreamPackage(uint associationId)
        {
            var error = _receiveStreamManager.EndStream(associationId);

            if (error.HasValue)
                SendError(error.Value);
        }

        protected long BeginSendPackage()
        {
            Debug.Assert(SendBuffer.Length == SendBuffer.Position);

            long position = SendBuffer.Position;

            // Make room for the header.

            SendBuffer.SetLength(SendBuffer.Length + 3);
            SendBuffer.Position = SendBuffer.Length;

            return position;
        }

        protected void EndSendPackage(PackageType packageType, long packageStart)
        {
            EndSendPackage(packageType, packageStart, true);
        }

        protected void EndSendPackage(PackageType packageType, long packageStart, bool forceSend)
        {
            Debug.Assert(SendBuffer.Length == SendBuffer.Position);

            long position = SendBuffer.Position;

            SendBuffer.Position = packageStart;

            long messageLength = position - packageStart - 3;

            uint messageHeader = (uint)messageLength << 3 | (uint)packageType;

            var buffer = BitConverter.GetBytes(messageHeader);

            ByteUtil.ConvertNetwork(buffer);

            SendBuffer.Write(buffer, 1, buffer.Length - 1);

            SendBuffer.Position = position;

            Debug.Assert(SendBuffer.Length == SendBuffer.Position);

            if (forceSend)
                Send();
        }

        protected void Send()
        {
            lock (_syncRoot)
            {
                if (IsAsync)
                    SendAsync();
                else
                    SendSync();
            }
        }

        private void SendSync()
        {
            // SendSync is only used in the connection phase of client connections.
            // We do not support sending streams here.

            RingMemoryPage page;

            while (TryGetSendPage(out page))
            {
                Stream.Write(page.Buffer, page.Offset, page.Count);

                SendBuffer.Head += page.Count;
            }
        }

        private void SendAsync()
        {
            ProcessSendRequests(false);
        }

        private void ProcessSendRequests(bool force)
        {
            if (Stream == null)
                return;

            if (force)
                _sending = false;

            if (
                !_sending &&
                (SendBuffer.Head != SendBuffer.Length || ProcessPendingStream())
            ) {
                Debug.Assert(SendBuffer.Head != SendBuffer.Length);

                _sending = true;

                var page = GetSendPage();

                Stream.BeginWrite(
                    page.Buffer, page.Offset, page.Count, WriteCallback, page
                );
            }
        }

        private bool ProcessPendingStream()
        {
            // If there isn't any data pending, see whether we can send
            // stream data.

            var sendRequest = _sendStreamManager.GetSendRequest();

            // If there is nothing to send, we can quit now.

            if (sendRequest == null)
                return false;

            if (sendRequest.Value.IsCompleted)
                SendStreamEnd(sendRequest.Value);
            else
                SendStreamData(sendRequest.Value);

            return true;
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

            long length = request.Length;

            while (length > 0)
            {
                Debug.Assert(SendBuffer.Length == SendBuffer.Position);

                // We write directly into the back buffers of the send buffer.

                long pageSize = Math.Min(
                    SendBuffer.BlockSize - SendBuffer.Position % SendBuffer.BlockSize, // Maximum size to stay on the page
                    length
                );

                // Make room for the page.

                SendBuffer.SetLength(SendBuffer.Length + pageSize);

                // Get the page we're using to write the stream data on.

                var page = SendBuffer.GetPage(SendBuffer.Position, pageSize);

                int read = request.Stream.Stream.Read(page.Buffer, page.Offset, page.Count);

                Debug.Assert(read == page.Count);

                // Move the position forward to correspond with the data
                // we've just written.

                SendBuffer.Position += pageSize;

                length -= page.Count;
            }

            // And send the package.

            EndSendPackage(PackageType.Stream, packageStart, false);
        }

        private void WriteStreamPackageHeader(StreamSendRequest request, StreamPackageType streamPackageType)
        {
            uint header = (uint)streamPackageType | request.Stream.AssociationId << 3;

            var buffer = BitConverter.GetBytes(header);

            ByteUtil.ConvertNetwork(buffer);

            SendBuffer.Write(buffer, 1, buffer.Length - 1);
        }

        private RingMemoryPage GetSendPage()
        {
            RingMemoryPage result;

            if (!TryGetSendPage(out result))
                throw new InvalidOperationException("No data to send");

            return result;
        }

        private bool TryGetSendPage(out RingMemoryPage page)
        {
            long pageSize = Math.Min(
                SendBuffer.BlockSize - SendBuffer.Head % SendBuffer.BlockSize, // Maximum size to stay on the page
                SendBuffer.Length - SendBuffer.Head // Maximum size to send at all
            );

            if (pageSize == 0)
            {
                page = new RingMemoryPage();

                return false;
            }
            else
            {
                page = SendBuffer.GetPage(SendBuffer.Head, pageSize);

                Debug.Assert(SendBuffer.Head + pageSize <= SendBuffer.Length);

                return true;
            }
        }

        private void WriteCallback(IAsyncResult ar)
        {
            lock (_syncRoot)
            {
                if (IsDisposed)
                    return;

                if (Stream != null)
                {
                    try
                    {
                        Stream.EndWrite(ar);

                        SendBuffer.Head += ((RingMemoryPage)ar.AsyncState).Count;

                        ProcessSendRequests(true);
                    }
                    catch
                    {
                        Dispose();
                    }
                }
            }
        }

        protected void SendError(ProtocolError error)
        {
            long packageStart = BeginSendPackage();

            ProtoBuf.Serializer.Serialize(SendBuffer, new Messages.Error
            {
                ErrorNumber = (uint)error
            });

            EndSendPackage(PackageType.Error, packageStart);
        }

        public uint SendStream(Stream stream, string streamName, string contentType)
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

            SendBuffer.Write(buffer, 1, buffer.Length - 1);

            // Write the details of the request.

            var message = new Messages.StartStream
            {
                Length = (uint)stream.Length,
                StreamName = streamName,
                ContentType = contentType
            };

            TypeModel.Serialize(SendBuffer, message);

            // Send the package.

            EndSendPackage(PackageType.Stream, packageStart);

            return associationId;
        }

        public ProtoStream GetStream(uint streamId)
        {
            return EndGetStream(BeginGetStream(streamId, null, null));
        }

        public IAsyncResult BeginGetStream(uint streamId, AsyncCallback callback, object asyncState)
        {
            return _receiveStreamManager.BeginGetStream(streamId, callback, asyncState);
        }

        public ProtoStream EndGetStream(IAsyncResult asyncResult)
        {
            return PendingReceiveStream.EndGetStream(asyncResult);
        }

        private void VerifyNotDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                if (TcpClient != null)
                {
                    TcpClient.Close();
                    TcpClient = null;
                }

                IsDisposed = true;
            }
        }

        protected struct PendingPackage
        {
            private readonly PackageType _type;
            private readonly uint _length;

            public PendingPackage(PackageType type, uint length)
            {
                _type = type;
                _length = length;
            }

            public PackageType Type
            {
                get { return _type; }
            }

            public uint Length
            {
                get { return _length; }
            }
        }
    }
}
