using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoChannel
{
    public class UnhandledExceptionEventArgs : EventArgs
    {
        public Exception Exception { get; private set; }

        public UnhandledExceptionEventArgs(Exception exception)
        {
            Require.NotNull(exception, "exception");

            Exception = exception;
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1003:UseGenericEventHandlerInstances")]
    public delegate void UnhandledExceptionEventHandler(object sender, UnhandledExceptionEventArgs e);
}
