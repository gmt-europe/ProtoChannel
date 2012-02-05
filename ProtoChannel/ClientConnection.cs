using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using ProtoChannel.Util;

namespace ProtoChannel
{
    internal class ClientConnection : ProtoConnection
    {
        private readonly ProtoClient _client;
        private readonly string _hostname;
        private State _state;
        private readonly PendingMessageManager _messageManager = new PendingMessageManager();

        public ClientConnection(ProtoClient client, TcpClient tcpClient, string hostname, IStreamManager streamManager)
            : base(tcpClient, streamManager)
        {
            if (client == null)
                throw new ArgumentNullException("client");
            if (hostname == null)
                throw new ArgumentNullException("hostname");

            _client = client;
            _hostname = hostname;

            Connect();
        }

        private void Connect()
        {
            if (_client.Configuration.Secure)
            {
                AuthenticateAsClient(
                    _client.Configuration.ValidationCallback,
                    _hostname
                );
            }

            _state = State.ReceivingHandshake;

            // Push the prolog header.

            Write(Constants.Header, 0, Constants.Header.Length);

            // Push the version number.

            var buffer = BitConverter.GetBytes((uint)Constants.ProtocolVersion);

            ByteUtil.ConvertNetwork(buffer);

            Write(buffer, 0, buffer.Length);

            // Send the buffer contents.

            Send();

            while (!IsDisposed && _state == State.ReceivingHandshake)
            {
                Read();
            }
        }

        protected override void ProcessPackage(ProtoConnection.PendingPackage package)
        {
            switch (_state)
            {
                case State.ReceivingHandshake:
                    if (package.Type != PackageType.Handshake)
                    {
                        SendError(ProtocolError.InvalidPackageType);
                        return;
                    }
                    break;

                case State.Connected:
                    break;

                default:
                    throw new NotImplementedException();
            }

            switch (package.Type)
            {
                case PackageType.Handshake:
                    ProcessHandshake(package);
                    break;

                default:
                    base.ProcessPackage(package);
                    break;
            }
        }

        private void ProcessHandshake(PendingPackage package)
        {
            // Receive the handshake request.

            var request = (Messages.HandshakeRequest)ReadMessage(
                TypeModel, typeof(Messages.HandshakeRequest), (int)package.Length
            );

            // Ask the client which protocol we're going to connect with.

            int protocol = _client.ChooseProtocol((int)request.ProtocolMin, (int)request.ProtocolMax);

            // Push our response

            long packageStart = BeginSendPackage();

            WriteMessage(TypeModel, new Messages.HandshakeResponse
            {
                Protocol = (uint)protocol
            });

            EndSendPackage(PackageType.Handshake, packageStart);

            _state = State.Connected;

            // Once we're connected, we can go into async mode.

            IsAsync = true;

            Read();
        }

        protected override void ProcessMessage(MessageKind kind, uint type, uint length, uint associationId)
        {
            if (kind == MessageKind.Response)
                ProcessResponseMessage(type, length, associationId);
            else
                ProcessRequestMessage(type, length, associationId, kind == MessageKind.OneWay);
        }

        private void ProcessRequestMessage(uint type, uint length, uint associationId, bool isOneWay)
        {
            throw new NotImplementedException();
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
                _client.ServiceAssembly.TypeModel, pendingMessage.MessageType.Type, (int)length
            );

            pendingMessage.SetAsCompleted(message, false);
        }

        public IAsyncResult BeginSendMessage(object message, Type responseType, AsyncCallback callback, object asyncState)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            ServiceMessage messageType;

            if (!_client.ServiceAssembly.MessagesByType.TryGetValue(message.GetType(), out messageType))
                throw new ProtoChannelException(String.Format("Message type '{0}' is not a valid message type", message.GetType()));

            ServiceMessage responseMessageType = null;

            if (responseType != null)
            {
                if (!_client.ServiceAssembly.MessagesByType.TryGetValue(responseType, out responseMessageType))
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

            WriteMessage(_client.ServiceAssembly.TypeModel, message);

            // Send the message.

            EndSendPackage(PackageType.Message, packageStart);

            return pendingMessage;
        }

        public object EndSendMessage(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
                throw new ArgumentNullException("asyncResult");

            return ((PendingMessage)asyncResult).EndInvoke();
        }

        public void PostMessage(object message)
        {
            throw new NotImplementedException();
        }

        private enum State
        {
            ReceivingHandshake,
            Connected
        }
    }
}
