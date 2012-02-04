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
    internal abstract class ProtoConnection : IDisposable
    {
        private readonly object _syncRoot = new object();
        private PendingPackage? _pendingPackage;
        private bool _sending;

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

        protected ProtoConnection(TcpClient tcpClient)
        {
            if (tcpClient == null)
                throw new ArgumentNullException("tcpClient");

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
            Debug.Assert(SendBuffer.Length == SendBuffer.Position);

            long position = SendBuffer.Position;

            SendBuffer.Position = packageStart;

            long messageLength = position - packageStart - 3;

            uint messageHeader = (uint)messageLength << 3 | (uint)packageType;

            var buffer = BitConverter.GetBytes(messageHeader);

            ByteUtil.ConvertNetwork(buffer);

            SendBuffer.Write(buffer, 1, buffer.Length - 1);

            SendBuffer.Position = position;

            Send();
        }

        protected void Send()
        {
            if (IsAsync)
                SendAsync();
            else
                SendSync();
        }

        private void SendSync()
        {
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

            if (!_sending && SendBuffer.Head != SendBuffer.Length)
            {
                _sending = true;

                var page = GetSendPage();

                Stream.BeginWrite(
                    page.Buffer, page.Offset, page.Count, WriteCallback, page
                );
            }
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
