using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace ProtoChannel.Messages
{
    [ProtoContract]
    internal class Error
    {
        [ProtoMember(1, IsRequired = true)]
        public uint ErrorNumber { get; set; }
    }
}
