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
            if (exception == null)
                throw new ArgumentNullException("exception");

            Exception = exception;
        }
    }

    public delegate void UnhandledExceptionEventHandler(object sender, UnhandledExceptionEventArgs e);
}
