using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace ProtoChannel.Messages
{
    [ProtoContract]
    internal class StartStream
    {
        [ProtoMember(1, IsRequired = true)]
        public uint Length { get; set; }

        [ProtoMember(2)]
        public string StreamName { get; set; }

        [ProtoMember(3)]
        public string ContentType { get; set; }
    }
}
