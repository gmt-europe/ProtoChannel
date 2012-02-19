using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ProtoChannel.Test.Services.Streaming
{
    internal class ServerService
    {
        [ProtoMethod]
        public StreamResponse RequestStream(StreamRequest request)
        {
            Console.WriteLine("Stream request received");

            var stream = new MemoryStream(
                Encoding.UTF8.GetBytes("Stream contents")
            );

            int id = OperationContext.Current.Connection.SendStream(
                stream, "Payload", "application/octet-stream"
            );

            return new StreamResponse { StreamId = (uint)id };
        }
    }
}
