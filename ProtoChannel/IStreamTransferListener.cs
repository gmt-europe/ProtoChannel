using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoChannel
{
    internal interface IStreamTransferListener
    {
        void RaiseStreamTransfer(PendingStream stream, StreamTransferEventType eventType);
    }
}
