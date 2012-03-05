using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using NUnit.Framework;
using ProtoChannel.Test.Service;

namespace ProtoChannel.Test.Infrastructure
{
    [TestFixture]
    public class InvalidServerService : FixtureBase
    {
        [Test]
        [ExpectedException]
        public void MultipleHandlersForMessage()
        {
            using (new ProtoHost<MultipleHandlersServerService>(new IPEndPoint(IPAddress.Loopback, 0)))
            {
            }
        }

        [Test]
        [ExpectedException]
        public void NoHandlersForMessage()
        {
            using (new ProtoHost<NoHandlersServerService>(new IPEndPoint(IPAddress.Loopback, 0)))
            {
            }
        }

        private class MultipleHandlersServerService
        {
            [ProtoMethod]
            public Pong Ping1(Ping message)
            {
                return new Pong { Payload = message.Payload };
            }
            [ProtoMethod]
            public Pong Ping2(Ping message)
            {
                return new Pong { Payload = message.Payload };
            }
        }

        private class NoHandlersServerService
        {
        }
    }
}
