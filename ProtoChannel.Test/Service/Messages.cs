using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace ProtoChannel.Test.Service
{
    [ProtoMessage(1), ProtoContract]
    public class Ping
    {
        [ProtoMember(1, IsRequired = true)]
        public string Payload { get; set; }
    }

    [ProtoMessage(2), ProtoContract]
    public class Pong
    {
        [ProtoMember(1, IsRequired = true)]
        public string Payload { get; set; }
    }

    [ProtoMessage(3), ProtoContract]
    public class StreamRequest
    {
    }

    [ProtoMessage(4), ProtoContract]
    public class StreamResponse
    {
        [ProtoMember(1, IsRequired = true)]
        public uint StreamId { get; set; }
    }

    [ProtoMessage(5), ProtoContract]
    public class OneWayPing
    {
        [ProtoMember(1, IsRequired = true)]
        public string Payload { get; set; }
    }

    [ProtoMessage(6), ProtoContract]
    public class DefaultValueTests
    {
        [ProtoMember(1), DefaultValue("Default value")]
        public string StringValue { get; set; }

        [ProtoMember(2), DefaultValue(1)]
        public int IntValue { get; set; }

        [ProtoMember(3), DefaultValue(1.0)]
        public double DoubleValue { get; set; }
    }
}
