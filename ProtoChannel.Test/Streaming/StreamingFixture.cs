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
            RequestStream(StreamDisposition.Attachment);
        }

        [Test]
        public void RequestInlineStream()
        {
            RequestStream(StreamDisposition.Inline);
        }

        public void RequestStream(StreamDisposition disposition)
        {
            using (var host = new ProtoHost<ServerService>(new IPEndPoint(IPAddress.Loopback, 0)))
            using (var client = new ClientService(host.LocalEndPoint))
            {
                var response = client.StreamRequest(new StreamRequest
                {
                    Attachment = disposition == StreamDisposition.Attachment
                });

                using (var protoStream = client.EndGetStream(client.BeginGetStream((int)response.StreamId, null, null)))
                {
                    Assert.AreEqual(protoStream.Disposition, disposition);

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
            SendStream(StreamDisposition.Attachment);
        }

        [Test]
        public void SendInlineStream()
        {
            SendStream(StreamDisposition.Inline);
        }

        private void SendStream(StreamDisposition disposition)
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
                    "text/plain",
                    disposition
                );

                client.StreamUpload(new StreamResponse { StreamId = (uint)streamId });

                Assert.True(callback.CallbackReceivedEvent.WaitOne(TimeSpan.FromSeconds(1)));
                Assert.True(callback.OneWayPingPayload.Contains(disposition.ToString()));
            }
        }

        [Test]
        public void LargeSendStream()
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
                    new MemoryStream(new byte[1 << 20]),
                    "Payload.txt",
                    "text/plain"
                );

                client.StreamUpload(new StreamResponse { StreamId = (uint)streamId });

                Assert.True(callback.CallbackReceivedEvent.WaitOne(TimeSpan.FromSeconds(1)));
            }
        }

        [Test]
        public void SendStreamWithInitialReadFailure()
        {
            SendWithStreamFailure(1 << 20, StreamFailureType.InitialRead, true);
        }

        [Test]
        public void SendStreamWithReadSecondBlockFailure()
        {
            SendWithStreamFailure(1 << 20, StreamFailureType.ReadSecondBlock, true);
        }

        [Test]
        public void SendStreamWithDisposeFailure()
        {
            SendWithStreamFailure(1 << 20, StreamFailureType.Dispose, false);
        }

        [Test]
        [ExpectedException]
        public void SendStreamWithReadPositionFailure()
        {
            SendWithStreamFailure(1 << 20, StreamFailureType.ReadPosition, true);
        }

        [Test]
        public void SendStreamWithInitialReadAndDisposeFailure()
        {
            SendWithStreamFailure(1 << 20, StreamFailureType.InitialRead | StreamFailureType.Dispose, true);
        }

        [Test]
        public void SendStreamWithReadSecondBlockAndDisposeFailure()
        {
            SendWithStreamFailure(1 << 20, StreamFailureType.ReadSecondBlock | StreamFailureType.Dispose, true);
        }

        private void SendWithStreamFailure(long length, StreamFailureType type, bool expectFailure)
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
                    new FailingStream(length, type),
                    "Payload.txt",
                    "text/plain"
                );

                client.StreamUpload(new StreamResponse { StreamId = (uint)streamId });

                Assert.True(callback.CallbackReceivedEvent.WaitOne(TimeSpan.FromSeconds(1)));

                if (expectFailure)
                    Assert.AreEqual("Receive stream failed", callback.OneWayPingPayload);
                else
                    Assert.AreNotEqual("Receive stream failed", callback.OneWayPingPayload);
            }
        }
    }
}
