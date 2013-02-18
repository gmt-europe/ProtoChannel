using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoChannel
{
    internal enum StreamPackageType
    {
        StartStream = 0,
        AcceptStream = 1,
        RejectStream = 2,
        StreamData = 3,
        EndStream = 4,
        StreamFailed = 5
    }
}
