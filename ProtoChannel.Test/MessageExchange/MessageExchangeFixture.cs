using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using NUnit.Framework;
using ProtoChannel.Test.Services.PingPong;

namespace ProtoChannel.Test.MessageExchange
{
    [TestFixture]
    internal class MessageExchangeFixture : FixtureBase
    {
        [Test]
        public void SendReceiveMessage()
        {
            using (var host = new ProtoHost<ServerService>(new IPEndPoint(IPAddress.Loopback, 0)))
            using (var client = new ClientService(host.LocalEndPoint))
            {
                var result = client.Ping(new Ping { Payload = "Hello" });

                Assert.IsNotNull(result);
                Assert.AreEqual("Hello", result.Payload);
            }
        }
    }
}
