using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using NUnit.Framework;
using ProtoChannel.Test.Services.PingPong;

namespace ProtoChannel.Test.ChannelSetup
{
    [TestFixture]
    internal class BasicConnection
    {
        [Test]
        public void Connect()
        {
            using (var host = new ProtoHost<ServerService>(new IPEndPoint(IPAddress.Loopback, 0)))
            {
                using (new ClientService(host.LocalEndPoint))
                {
                }
            }
        }
    }
}
