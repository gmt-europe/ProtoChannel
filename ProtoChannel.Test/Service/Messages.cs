﻿using System;
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
        public DefaultValueTests()
        {
            StringValue = "Default value";
            IntValue = 1;
            DoubleValue = 1.0;
        }

        [ProtoMember(1), DefaultValue("Default value")]
        public string StringValue { get; set; }

        [ProtoMember(2), DefaultValue(1)]
        public int IntValue { get; set; }

        [ProtoMember(3), DefaultValue(1.0)]
        public double DoubleValue { get; set; }
    }

    [ProtoMessage(7), ProtoContract]
    public class StringArrayTest
    {
        [ProtoMember(1)]
        public string[] Values { get; set; }
    }

    [ProtoMessage(8), ProtoContract]
    public class IntArrayTest
    {
        [ProtoMember(1)]
        public int[] Values { get; set; }
    }

    [ProtoMessage(9), ProtoContract]
    public class NestedTypeTest
    {
        [ProtoMember(1)]
        public NestedType Value { get; set; }
    }

    [ProtoMessage(10), ProtoContract]
    public class NestedTypeArrayTest
    {
        [ProtoMember(1)]
        public NestedType[] Values { get; set; }
    }

    [ProtoContract]
    public class NestedType
    {
        [ProtoMember(1)]
        public string Value { get; set; }
    }
}
