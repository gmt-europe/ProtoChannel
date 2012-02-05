using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace ProtoChannel.Test.Services.Streaming
{
    [ProtoMessage(3), ProtoContract]
    internal class StreamRequest
    {
    }

    [ProtoMessage(4), ProtoContract]
    internal class StreamResponse
    {
        [ProtoMember(1, IsRequired = true)]
        public uint StreamId { get; set; }
    }
}
