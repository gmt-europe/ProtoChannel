using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;

namespace ProtoChannel.Test.Infrastructure
{
    internal class RogueClient : IDisposable
    {
        private TcpClient _connection;
        private bool _disposed;
        private Stream _stream;

        public void Connect(IPEndPoint endPoint, bool secure, bool rogue)
        {
            _connection = new TcpClient();

            _connection.Connect(endPoint);

            if (secure)
            {
                if (rogue)
                {
                    _stream = _connection.GetStream();

                    SendGarbage();
                }
                else
                {
                    var sslStream = new SslStream(_connection.GetStream(), false, (p1, p2, p3, p4) => true);

                    sslStream.AuthenticateAsClient(
                        "localhost",
                        null,
                        SslProtocols.Tls,
                        false
                    );

                    _stream = sslStream;
                }
            }
        }

        public void WaitForRead()
        {
            _stream.ReadByte();
        }

        private void SendGarbage()
        {
            var garbage = new byte[100];
            new Random().NextBytes(garbage);

            _stream.Write(garbage, 0, garbage.Length);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_connection != null)
                {
                    _connection.Close();
                    _connection = null;
                }

                _disposed = true;
            }
        }
    }
}
