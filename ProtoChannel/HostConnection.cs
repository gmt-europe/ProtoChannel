using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using ProtoChannel.Util;

namespace ProtoChannel
{
    internal class HostConnection : ProtoConnection
    {
        private readonly ProtoHost _host;
        private State _state;
        private SslStream _sslStream;
        private bool _disposed;

        public HostConnection(ProtoHost host, TcpClient tcpClient, IStreamManager streamManager)
            : base(tcpClient, streamManager, host.ServiceAssembly)
        {
            Require.NotNull(host, "host");

            _host = host;

            IsAsync = true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void Connect()
        {
            try
            {
                if (_host.Configuration.Secure)
                {
                    _state = State.Authenticating;

                    BeginAuthenticateAsServer(
                        _host.Configuration.Certificate,
                        _host.Configuration.ValidationCallback,
                        AuthenticateAsServerCallback,
                        null
                    );
                }
                else
                {
                    _state = State.ReceivingProlog;

                    Read();
                }
            }
            catch (Exception ex)
            {
                _host.RaiseUnhandledException(this, ex);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AuthenticateAsServerCallback(IAsyncResult asyncResult)
        {
            try
            {
                EndAuthenticateAsServer(asyncResult);

                _state = State.ReceivingProlog;

                Read();
            }
            catch (Exception ex)
            {
                _host.RaiseUnhandledException(this, ex);
            }
        }

        protected override bool ProcessInput()
        {
            // Locked by TcpConnection.

            if (_state == State.ReceivingProlog)
            {
                // Is there enough data in the buffer?

                if (ReadAvailable >= 8)
                {
                    byte[] header = new byte[8];

                    Read(header, 0, header.Length);

                    // Verify the magic.

                    if (!ByteUtil.Equals(header, Constants.Header, 4))
                    {
                        SendError(ProtocolError.InvalidProtocolHeader);
                        return false;
                    }

                    // Verify the protocol number.

                    uint protocolVersion = BitConverterEx.ToNetworkUInt32(header, 4);

                    if (protocolVersion != Constants.ProtocolVersion)
                    {
                        SendError(ProtocolError.InvalidProtocol);
                        return false;
                    }

                    _state = State.ReceivingHandshake;

                    SendHandshake();

                    return true;
                }
                else
                {
                    // Wait for more data to come in.

                    return false;
                }
            }
            else
            {
                return base.ProcessInput();
            }
        }

        private void SendHandshake()
        {
            long packageStart = BeginSendPackage();

            WriteMessage(TypeModel, new Messages.HandshakeRequest
            {
                ProtocolMin = (uint)_host.Configuration.MinimumProtocolNumber,
                ProtocolMax = (uint)_host.Configuration.MaximumProtocolNumber
            });

            EndSendPackage(PackageType.Handshake, packageStart);

            _state = State.Connected;
        }

        protected override void ProcessPackage(PendingPackage package)
        {
            // Locked by TcpConnection.

            switch (_state)
            {
                case State.Authenticating:
                case State.ReceivingProlog:
                    SendError(ProtocolError.InvalidPackageType);
                    return;

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
                    throw new NotSupportedException("Invalid state");
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
            // Receive the handshake response.

            var response = (Messages.HandshakeResponse)ReadMessage(
                TypeModel, typeof(Messages.HandshakeResponse), (int)package.Length
            );

            // Validate the protocol number.

            int protocolNumber = (int)response.Protocol;

            if (
                protocolNumber < _host.Configuration.MinimumProtocolNumber ||
                protocolNumber > _host.Configuration.MaximumProtocolNumber
            ) {
                SendError(ProtocolError.InvalidProtocol);
            }
            else
            {
                // Else, we've got a valid connection and can proceed with
                // creating the service client.

                // Create the callback channel so we can provide it with the
                // operation context.

                if (_host.Service.CallbackContractType != null)
                {
                    // Else, if we have a callback contract, we create the
                    // channel.

                    CallbackChannel = (ProtoCallbackChannel)Activator.CreateInstance(
                        _host.Service.CallbackContractType
                    );

                    CallbackChannel.Connection = this;
                }

                using (OperationContext.SetScope(new OperationContext(this, CallbackChannel)))
                {
                    Client = _host.RaiseClientConnected(this, protocolNumber);
                }

                if (Client == null)
                {
                    // When creating the client failed, we shut down because there's
                    // nothing more to do.

                    Dispose();
                }
            }
        }

        protected override void RaiseUnhandledException(Exception exception)
        {
            _host.RaiseUnhandledException(this, exception);
        }

        protected override void Dispose(bool disposing)
        {
            lock (SyncRoot)
            {
                if (!_disposed)
                {
                    if (_sslStream != null)
                    {
                        _sslStream.Dispose();
                        _sslStream = null;
                    }

                    _host.RemoveConnection(this);

                    _disposed = true;
                }
            }

            base.Dispose(disposing);
        }

        private enum State
        {
            Authenticating,
            ReceivingProlog,
            ReceivingHandshake,
            Connected
        }
    }
}
