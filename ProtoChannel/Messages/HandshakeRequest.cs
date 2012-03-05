using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace ProtoChannel.Messages
{
    [ProtoContract]
    internal class HandshakeRequest
    {
        [ProtoMember(1, IsRequired = true)]
        public uint ProtocolMin { get; set; }

        [ProtoMember(2, IsRequired = true)]
        public uint ProtocolMax { get; set; }
    }
}
