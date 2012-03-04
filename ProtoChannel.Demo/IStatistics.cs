using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoChannel.Demo
{
    internal interface IStatistics
    {
        void AddSendMessage(long ticks);
    }
}
