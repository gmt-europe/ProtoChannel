using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using ProtoBuf;

namespace ProtoChannel.Demo.Shared
{
    [ProtoMessage(1), ProtoContract]
#if _NET_4
    [DataContract]
#endif
    public class SimpleMessage
    {
        [ProtoMember(1, IsRequired = true)]
#if _NET_4
        [DataMember]
#endif
        public int Value { get; set; }
    }

    [ProtoMessage(2), ProtoContract]
#if _NET_4
    [DataContract]
#endif
    public class ComplexMessage
    {
        public ComplexMessage()
        {
            Values = new List<ComplexValue>();
        }

        [ProtoMember(1, IsRequired = true)]
#if _NET_4
        [DataMember]
#endif
        public List<ComplexValue> Values { get; private set; }
    }

    [ProtoContract]
#if _NET_4
    [DataContract]
#endif
    public class ComplexValue
    {
        [ProtoMember(1, IsRequired = true)]
#if _NET_4
        [DataMember]
#endif
        public int IntValue { get; set; }

        [ProtoMember(2, IsRequired = true)]
#if _NET_4
        [DataMember]
#endif
        public string StringValue { get; set; }

        [ProtoMember(3, IsRequired = true)]
#if _NET_4
        [DataMember]
#endif
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
