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
    public class UnhandledExceptionTestFixture : FixtureBase
    {
        [Test]
        public void InAuthenticateAsServer()
        {
            var configuration = new ProtoHostConfiguration
            {
                Secure = true,
                Certificate = GetCertificate()
            };

            using (var host = new UnhandledExceptionHost(new IPEndPoint(IPAddress.Loopback, 0), configuration))
            using (var client = new RogueClient())
            {
                client.Connect(host.LocalEndPoint, true, true);

                Assert.IsTrue(host.HadExceptionEvent.WaitOne(TimeSpan.FromSeconds(1)));
            }
        }

        [Test]
        public void InHandleRequest()
        {
            using (var host = new UnhandledExceptionHost(new IPEndPoint(IPAddress.Loopback, 0), null))
            using (var client = new ClientService(host.LocalEndPoint))
            {
                try
                {
                    client.ThrowingMethod(new ThrowingTest());

                    Assert.Fail("Expected exception");
                }
                catch
                {
                }

                Assert.IsTrue(host.HadExceptionEvent.WaitOne(TimeSpan.FromSeconds(1)));
            }
        }

        private class UnhandledExceptionHost : ProtoHost<ServerService>
        {
            public ManualResetEvent HadExceptionEvent { get; private set; }

            public UnhandledExceptionHost(IPEndPoint localEndPoint, ProtoHostConfiguration configuration)
                : base(localEndPoint, configuration)
            {
                HadExceptionEvent = new ManualResetEvent(false);
            }

            protected override void OnUnhandledException(UnhandledExceptionEventArgs e)
            {
                HadExceptionEvent.Set();
            }
        }
    }
}
