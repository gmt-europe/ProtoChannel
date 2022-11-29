using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using NUnit.Framework;
using ProtoChannel.Test.Service;

namespace ProtoChannel.Test.ChannelSetup
{
    [TestFixture]
    internal class BasicConnection : FixtureBase
    {
        [Test]
        public void Connect()
        {
            using (var host = new ProtoHost<ServerService>(new IPEndPoint(IPAddress.Loopback, 0)))
            using (new ClientService(host.LocalEndPoint))
            {
            }
        }

        [Test]
        public void SecureConnect()
        {
            var hostConfig = new ProtoHostConfiguration
            {
                Secure = true,
                Certificate = GetCertificate()
            };

            var clientConfig = new ProtoClientConfiguration
            {
                Secure = true
            };

            using (var host = new ProtoHost<ServerService>(new IPEndPoint(IPAddress.Loopback, 0), hostConfig))
            using (new ClientService(host.LocalEndPoint, clientConfig))
            {
            }
        }

        [Test]
        public void SecureConnectWithTls12Server()
        {
            var hostConfig = new ProtoHostConfiguration
            {
                Secure = true,
                Certificate = GetCertificate(),
                SecurityProtocol = (System.Security.Authentication.SslProtocols)3072 // This is TLS1.2
            };
 
            var clientConfig = new ProtoClientConfiguration
            {
                Secure = true,
                SecurityProtocol = unchecked((System.Security.Authentication.SslProtocols)0xffffffff) // Configure the client to support any TLS version
            };

            using (var host = new ProtoHost<ServerService>(new IPEndPoint(IPAddress.Loopback, 0), hostConfig))
            using (new ClientService(host.LocalEndPoint, clientConfig))
            {
            }
        }
    }
}
