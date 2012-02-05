using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TraceListeners;

namespace ProtoChannel.Test
{
    internal class FixtureBase
    {
        static FixtureBase()
        {
            // Remove the default trace listener. The default trace listener
            // throws up a dialog, and that doesn't work well when unit testing.

            foreach (TraceListener listener in Debug.Listeners)
            {
                if (listener is DefaultTraceListener)
                {
                    Debug.Listeners.Remove(listener);
                    break;
                }
            }

            // Instead we insert a trace listener that does a debug break where
            // an assert fails.

            Debug.Listeners.Add(new DebugBreakListener());
        }
    }
}
