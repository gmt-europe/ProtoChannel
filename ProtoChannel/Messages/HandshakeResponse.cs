using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace ProtoChannel.Messages
{
    [ProtoContract]
    internal class HandshakeResponse
    {
        [ProtoMember(1, IsRequired = true)]
        public uint Protocol { get; set; }
    }
}
