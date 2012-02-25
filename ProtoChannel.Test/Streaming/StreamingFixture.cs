using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

                var stream = client.EndGetStream(client.BeginGetStream((int)response.StreamId, null, null));

                using (var reader = new StreamReader(stream.Stream))
                {
                    Console.WriteLine(reader.ReadToEnd());
                }
            }
        }
    }
}
