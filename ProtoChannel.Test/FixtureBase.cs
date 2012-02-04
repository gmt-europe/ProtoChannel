using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ProtoChannel.Test
{
    internal class FixtureBase
    {
        static FixtureBase()
        {
            foreach (TraceListener listener in Debug.Listeners)
            {
                if (listener is DefaultTraceListener)
                {
                    Debug.Listeners.Remove(listener);
                    break;
                }
            }
        }
    }
}
