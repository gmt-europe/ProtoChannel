using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Common.Logging;
using Common.Logging.Simple;
using TraceListeners;

namespace ProtoChannel.Test
{
    public class FixtureBase
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

            // Setup Common.Logging.

            LogManager.Adapter = new ConsoleOutLoggerFactoryAdapter();
        }

        protected X509Certificate2 GetCertificate()
        {
            string resourceName = typeof(FixtureBase).Namespace + ".testcert.pfx";

            using (var inputStream = GetType().Assembly.GetManifestResourceStream(resourceName))
            using (var outputStream = new MemoryStream())
            {
                inputStream.CopyTo(outputStream);

                return new X509Certificate2(outputStream.ToArray());
            }
        }
    }
}
