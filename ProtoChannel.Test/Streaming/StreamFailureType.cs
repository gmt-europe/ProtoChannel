using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoChannel.Test.Streaming
{
    [Flags]
    internal enum StreamFailureType
    {
        ReadPosition = 1,
        InitialRead = 2,
        ReadSecondBlock = 4,
        Dispose = 8
    }
}
