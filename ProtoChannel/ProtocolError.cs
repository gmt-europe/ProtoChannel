using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoChannel
{
    internal enum ProtocolError
    {
        UnknownError = 0,
        InvalidPackageType = 1,
        ProtocolUnsupported = 2,
        InvalidPackageLength = 3,
        UnexpectedStreamPackageType = 4,
        InvalidStreamData = 5,
        InvalidStreamPackageType = 6,
        InvalidProtocolHeader = 7,
        InvalidProtocol = 8
    }
}
