using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using NUnit.Framework;
using ProtoChannel.Test.Service;

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

        [Test]
        public void OneWay()
        {
            var callback = new ClientCallbackService();

            var clientConfig = new ProtoClientConfiguration
            {
                CallbackObject = callback
            };

            using (var host = new ProtoHost<ServerService>(new IPEndPoint(IPAddress.Loopback, 0)))
            using (var client = new ClientService(host.LocalEndPoint, clientConfig))
            {
                client.OneWayPing(new OneWayPing());

                callback.CallbackReceivedEvent.WaitOne();
            }
        }

        [Test]
        public void ReverseReceiveMessage()
        {
            var callback = new ClientCallbackService();

            var clientConfig = new ProtoClientConfiguration
            {
                CallbackObject = callback
            };

            using (var host = new ReverseServiceHost(new IPEndPoint(IPAddress.Loopback, 0)))
            using (new ClientService(host.LocalEndPoint, clientConfig))
            {
                callback.CallbackReceivedEvent.WaitOne();

                host.ResponseReceived.WaitOne();
            }
        }

        private class ReverseServiceHost : ProtoHost<ServerService>
        {
            public ManualResetEvent ResponseReceived { get; private set; }

            public ReverseServiceHost(IPEndPoint localEndPoint)
                : base(localEndPoint)
            {
                ResponseReceived = new ManualResetEvent(false);
            }

            protected override ServerService CreateService(int protocolNumber)
            {
                var service = base.CreateService(protocolNumber);

                var callbackChannel = OperationContext.Current.GetCallbackChannel<ServerCallbackService>();

                // The callback call cannot be done inline because we aren't
                // able to send out data because the connection is currently
                // locked. The response will never come in and EndSendMessage
                // will never return.

                ThreadPool.QueueUserWorkItem(
                    p =>
                    {
                        callbackChannel.Ping(new Ping { Payload = "Reverse ping" });
                        ResponseReceived.Set();
                    },
                    null
                );

                return service;
            }
        }
    }
}
