using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoChannel
{
    internal enum PackageType
    {
        NoOp = 0,
        Error = 1,
        Handshake = 2,
        Message = 3,
        Stream = 4,
        Ping = 5,
        Pong = 6
    }
}
