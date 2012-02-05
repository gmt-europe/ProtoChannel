﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using NUnit.Framework;
using ProtoChannel.Test.Services.Streaming;

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
                var response = client.RequestStream(new StreamRequest());

                var stream = client.GetStream(response.StreamId);

                using (var reader = new StreamReader(stream.Stream))
                {
                    Console.WriteLine(reader.ReadToEnd());
                }
            }
        }
    }
}
