using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace ProtoChannel.Demo.ProtoService
{
    [ProtoMessage(1), ProtoContract]
    public class SimpleMessage
    {
        [ProtoMember(1, IsRequired = true)]
        public int Value { get; set; }
    }
}
