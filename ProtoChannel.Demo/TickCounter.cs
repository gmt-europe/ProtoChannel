using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ProtoChannel.Demo
{
    internal class TickCounter
    {
        public long Total { get; private set; }
        public long Count { get; private set; }

        public void Add(long ticks)
        {
            Count++;
            Total += ticks;
        }

        public double Value
        {
            get
            {
                if (Count == 0)
                    return 0;
                else
                    return ((double)(Total / Count) / Stopwatch.Frequency) * 1000d;
            }
        }
    }
}
