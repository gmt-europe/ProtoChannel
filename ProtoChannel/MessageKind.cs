using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoChannel
{
    internal enum MessageKind
    {
        OneWay = 0,
        Request = 1,
        Response = 2
    }
}
