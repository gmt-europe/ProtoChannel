using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using NUnit.Framework;
using ProtoChannel.Test.Service;

namespace ProtoChannel.Test.Infrastructure
{
    [TestFixture]
    public class OperationContextFixture : FixtureBase
    {
        [Test]
        public void ValidateCallbackChannel()
        {
            using (var host = new IllegalCallbackServiceHost(new IPEndPoint(IPAddress.Loopback, 0)))
            using (new ClientService(host.LocalEndPoint))
            {
                Assert.IsTrue(host.HadExceptionEvent.WaitOne(TimeSpan.FromSeconds(1)));
            }
        }

        [Test]
        public void CallbackChannelAvailable()
        {
            using (var host = new NoCallbackAvailableServiceHost(new IPEndPoint(IPAddress.Loopback, 0)))
            using (new ClientService(host.LocalEndPoint))
            {
                Assert.IsTrue(host.HadExceptionEvent.WaitOne(TimeSpan.FromSeconds(1)));
            }
        }

        private class IllegalCallbackServiceHost : ProtoHost<ServerService>
        {
            public ManualResetEvent HadExceptionEvent { get; private set; }

            public IllegalCallbackServiceHost(IPEndPoint localEndPoint)
                : base(localEndPoint)
            {
                HadExceptionEvent = new ManualResetEvent(false);
            }

            protected override ServerService CreateService(int protocolNumber)
            {
                var service = base.CreateService(protocolNumber);

                try
                {
                    OperationContext.Current.GetCallbackChannel<DummyType>();
                }
                catch
                {
                    HadExceptionEvent.Set();
                }

                return service;
            }

            private class DummyType : ProtoCallbackChannel
            {
            }
        }

        private class NoCallbackServerService
        {
            [ProtoMethod]
            public Pong Ping(Ping message)
            {
                return new Pong { Payload = message.Payload };
            }
        }

        private class NoCallbackAvailableServiceHost : ProtoHost<NoCallbackServerService>
        {
            public ManualResetEvent HadExceptionEvent { get; private set; }

            public NoCallbackAvailableServiceHost(IPEndPoint localEndPoint)
                : base(localEndPoint)
            {
                HadExceptionEvent = new ManualResetEvent(false);
            }

            protected override NoCallbackServerService CreateService(int protocolNumber)
            {
                var service = base.CreateService(protocolNumber);

                try
                {
                    OperationContext.Current.GetCallbackChannel<ProtoCallbackChannel>();
                }
                catch
                {
                    HadExceptionEvent.Set();
                }

                return service;
            }
        }
    }
}
