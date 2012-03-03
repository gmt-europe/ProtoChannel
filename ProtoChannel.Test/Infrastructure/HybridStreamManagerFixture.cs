using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using NUnit.Framework;
using ProtoChannel.Test.Service;

namespace ProtoChannel.Test.Infrastructure
{
    [TestFixture]
    public class HybridStreamManagerFixture : FixtureBase
    {
        [Test]
        [ExpectedException]
        public void InvalidDiskStreamPath()
        {
            new DiskStreamManager(Guid.NewGuid().ToString());
        }

        [Test]
        [ExpectedException]
        public void InvalidHybridStream()
        {
            new HybridStreamManager(Path.GetTempPath(), 100, 0);
        }

        [Test]
        public void ExpectMemoryStream()
        {
            var configuration = new ProtoClientConfiguration
            {
                StreamManager = new HybridStreamManager(Path.GetTempPath(), 50)
            };

            using (var host = new ProtoHost<ServerService>(new IPEndPoint(IPAddress.Loopback, 0)))
            using (var client = new ClientService(host.LocalEndPoint, configuration))
            {
                var response = client.StreamRequest(new StreamRequest { Length = 10 });

                using (var stream = client.EndGetStream(client.BeginGetStream((int)response.StreamId, null, null)))
                {
                    Assert.That(stream.Stream, Is.InstanceOf<MemoryStream>());
                }
            }
        }

        [Test]
        public void ExpectDiskStream()
        {
            var configuration = new ProtoClientConfiguration
            {
                StreamManager = new HybridStreamManager(Path.GetTempPath(), 50)
            };

            using (var host = new ProtoHost<ServerService>(new IPEndPoint(IPAddress.Loopback, 0)))
            using (var client = new ClientService(host.LocalEndPoint, configuration))
            {
                var response = client.StreamRequest(new StreamRequest { Length = 100 });

                using (var stream = client.EndGetStream(client.BeginGetStream((int)response.StreamId, null, null)))
                {
                    Assert.That(stream.Stream, Is.InstanceOf<FileStream>());
                }
            }
        }

        [Test]
        [ExpectedException]
        public void StreamTooLong()
        {
            var configuration = new ProtoClientConfiguration
            {
                StreamManager = new HybridStreamManager(Path.GetTempPath(), 50, 100)
            };

            using (var host = new ProtoHost<ServerService>(new IPEndPoint(IPAddress.Loopback, 0)))
            using (var client = new ClientService(host.LocalEndPoint, configuration))
            {
                // TODO: Communicating that the stream was rejected isn't yet
                // communicated clearly.

                var response = client.StreamRequest(new StreamRequest { Length = 200 });

                using (client.EndGetStream(client.BeginGetStream((int)response.StreamId, null, null)))
                {
                }
            }
        }
    }
}
