using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProtoChannel.HttpProxy.Service.PingPong
{
    public class ClientConfiguration : ProtoClientConfiguration
    {
        public int ProtocolVersion { get; set; }
    }
}