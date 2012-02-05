using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace ProtoChannel.Test.Services.Streaming
{
    [ProtoMessage(3)]
    internal class StreamRequest
    {
    }

    [ProtoMessage(4)]
    internal class StreamResponse
    {
        [ProtoMember(1, IsRequired = true)]
        public uint StreamId { get; set; }
    }
}
