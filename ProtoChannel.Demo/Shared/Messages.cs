using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ProtoBuf;

namespace ProtoChannel.Demo.Shared
{
    [ProtoMessage(1), ProtoContract, DataContract]
    public class SimpleMessage
    {
        [ProtoMember(1, IsRequired = true), DataMember]
        public int Value { get; set; }
    }

    [ProtoMessage(2), ProtoContract, DataContract]
    public class ComplexMessage
    {
        public ComplexMessage()
        {
            Values = new List<ComplexValue>();
        }

        [ProtoMember(1, IsRequired = true), DataMember]
        public List<ComplexValue> Values { get; private set; }
    }

    [ProtoContract, DataContract]
    public class ComplexValue
    {
        [ProtoMember(1, IsRequired = true), DataMember]
        public int IntValue { get; set; }

        [ProtoMember(2, IsRequired = true), DataMember]
        public string StringValue { get; set; }

        [ProtoMember(3, IsRequired = true), DataMember]
        public double DoubleValue { get; set; }
    }

    [ProtoMessage(3), ProtoContract]
    public class StreamMessage
    {
        [ProtoMember(1, IsRequired = true)]
        public uint StreamId { get; set; }
    }

    [ProtoMessage(4), ProtoContract]
    public class StreamReceivedMessage
    {
    }
}
