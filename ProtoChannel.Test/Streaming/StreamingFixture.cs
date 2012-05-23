using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using NUnit.Framework;
using ProtoChannel.Test.Service;

namespace ProtoChannel.Test.Streaming
{
    [TestFixture]
    internal class StreamingFixture : FixtureBase
    {
        [Test]
        public void RequestStream()
        {
            using (var host = new ProtoHost<ServerService>(new IPEndPoint(IPAddress.Loopback, 0)))
            using (var client = new ClientService(host.LocalEndPoint))
            {
                var response = client.StreamRequest(new StreamRequest());

                using (var protoStream = client.EndGetStream(client.BeginGetStream((int)response.StreamId, null, null)))
                {
                    using (var stream = protoStream.DetachStream())
                    using (var reader = new StreamReader(stream))
                    {
                        Console.WriteLine(reader.ReadToEnd());
                    }
                }
            }
        }

        [Test]
        public void SendStream()
        {
            var callback = new ClientCallbackService();

            var configuration = new ProtoClientConfiguration
            {
                CallbackObject = callback
            };

            using (var host = new ProtoHost<ServerService>(new IPEndPoint(IPAddress.Loopback, 0)))
            using (var client = new ClientService(host.LocalEndPoint, configuration))
            {
                int streamId = client.SendStream(
                    new MemoryStream(Encoding.UTF8.GetBytes("Payload")),
                    "Payload.txt",
                    "text/plain"
                );

                client.StreamUpload(new StreamResponse { StreamId = (uint)streamId });

                Assert.True(callback.CallbackReceivedEvent.WaitOne(TimeSpan.FromSeconds(1)));
            }
        }
    }
}
