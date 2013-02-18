#if _NET_MD

using System;
using System.Collections.Generic;
using System.Text;

// This is a fake implementation of Common.Logging for MonoDroid. This is here
// so we don't have to have a lot of #if's. The implementation doesn't do
// anything.

namespace Common.Logging
{
    internal interface ILog
    {
        void Trace(object message);
        void Trace(object message, Exception exception);
        void TraceFormat(string format, params object[] args);
        void TraceFormat(string format, Exception exception, params object[] args);
        void TraceFormat(IFormatProvider formatProvider, string format, params object[] args);
        void TraceFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args);
        void Trace(Action<FormatMessageHandler> formatMessageCallback);
        void Trace(Action<FormatMessageHandler> formatMessageCallback, Exception exception);
        void Trace(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback);
        void Trace(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception);
        void Debug(object message);
        void Debug(object message, Exception exception);
        void DebugFormat(string format, params object[] args);
        void DebugFormat(string format, Exception exception, params object[] args);
        void DebugFormat(IFormatProvider formatProvider, string format, params object[] args);
        void DebugFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args);
        void Debug(Action<FormatMessageHandler> formatMessageCallback);
        void Debug(Action<FormatMessageHandler> formatMessageCallback, Exception exception);
        void Debug(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback);
        void Debug(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception);
        void Info(object message);
        void Info(object message, Exception exception);
        void InfoFormat(string format, params object[] args);
        void InfoFormat(string format, Exception exception, params object[] args);
        void InfoFormat(IFormatProvider formatProvider, string format, params object[] args);
        void InfoFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args);
        void Info(Action<FormatMessageHandler> formatMessageCallback);
        void Info(Action<FormatMessageHandler> formatMessageCallback, Exception exception);
        void Info(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback);
        void Info(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception);
        void Warn(object message);
        void Warn(object message, Exception exception);
        void WarnFormat(string format, params object[] args);
        void WarnFormat(string format, Exception exception, params object[] args);
        void WarnFormat(IFormatProvider formatProvider, string format, params object[] args);
        void WarnFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args);
        void Warn(Action<FormatMessageHandler> formatMessageCallback);
        void Warn(Action<FormatMessageHandler> formatMessageCallback, Exception exception);
        void Warn(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback);
        void Warn(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception);
        void Error(object message);
        void Error(object message, Exception exception);
        void ErrorFormat(string format, params object[] args);
        void ErrorFormat(string format, Exception exception, params object[] args);
        void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args);
        void ErrorFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args);
        void Error(Action<FormatMessageHandler> formatMessageCallback);
        void Error(Action<FormatMessageHandler> formatMessageCallback, Exception exception);
        void Error(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback);
        void Error(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception);
        void Fatal(object message);
        void Fatal(object message, Exception exception);
        void FatalFormat(string format, params object[] args);
        void FatalFormat(string format, Exception exception, params object[] args);
        void FatalFormat(IFormatProvider formatProvider, string format, params object[] args);
        void FatalFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args);
        void Fatal(Action<FormatMessageHandler> formatMessageCallback);
        void Fatal(Action<FormatMessageHandler> formatMessageCallback, Exception exception);
        void Fatal(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback);
        void Fatal(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception);
        bool IsTraceEnabled { get; }
        bool IsDebugEnabled { get; }
        bool IsErrorEnabled { get; }
        bool IsFatalEnabled { get; }
        bool IsInfoEnabled { get; }
        bool IsWarnEnabled { get; }
    }

    public delegate string FormatMessageHandler(string format, params object[] args);

    internal static class LogManager
    {
        internal static ILog GetLogger(Type type)
        {
            return new DummyLog();
        }

        private class DummyLog : ILog
        {
            public void Trace(object message)
            {
            }

            public void Trace(object message, Exception exception)
            {
            }

            public void TraceFormat(string format, params object[] args)
            {
            }

            public void TraceFormat(string format, Exception exception, params object[] args)
            {
            }

            public void TraceFormat(IFormatProvider formatProvider, string format, params object[] args)
            {
            }

            public void TraceFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
            {
            }

            public void Trace(Action<FormatMessageHandler> formatMessageCallback)
            {
            }

            public void Trace(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
            {
            }

            public void Trace(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
            {
            }

            public void Trace(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
            {
            }

            public void Debug(object message)
            {
            }

            public void Debug(object message, Exception exception)
            {
            }

            public void DebugFormat(string format, params object[] args)
            {
            }

            public void DebugFormat(string format, Exception exception, params object[] args)
            {
            }

            public void DebugFormat(IFormatProvider formatProvider, string format, params object[] args)
            {
            }

            public void DebugFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
            {
            }

            public void Debug(Action<FormatMessageHandler> formatMessageCallback)
            {
            }

            public void Debug(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
            {
            }

            public void Debug(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
            {
            }

            public void Debug(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
            {
            }

            public void Info(object message)
            {
            }

            public void Info(object message, Exception exception)
            {
            }

            public void InfoFormat(string format, params object[] args)
            {
            }

            public void InfoFormat(string format, Exception exception, params object[] args)
            {
            }

            public void InfoFormat(IFormatProvider formatProvider, string format, params object[] args)
            {
            }

            public void InfoFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
            {
            }

            public void Info(Action<FormatMessageHandler> formatMessageCallback)
            {
            }

            public void Info(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
            {
            }

            public void Info(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
            {
            }

            public void Info(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
            {
            }

            public void Warn(object message)
            {
            }

            public void Warn(object message, Exception exception)
            {
            }

            public void WarnFormat(string format, params object[] args)
            {
            }

            public void WarnFormat(string format, Exception exception, params object[] args)
            {
            }

            public void WarnFormat(IFormatProvider formatProvider, string format, params object[] args)
            {
            }

            public void WarnFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
            {
            }

            public void Warn(Action<FormatMessageHandler> formatMessageCallback)
            {
            }

            public void Warn(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
            {
            }

            public void Warn(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
            {
            }

            public void Warn(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
            {
            }

            public void Error(object message)
            {
            }

            public void Error(object message, Exception exception)
            {
            }

            public void ErrorFormat(string format, params object[] args)
            {
            }

            public void ErrorFormat(string format, Exception exception, params object[] args)
            {
            }

            public void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args)
            {
            }

            public void ErrorFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
            {
            }

            public void Error(Action<FormatMessageHandler> formatMessageCallback)
            {
            }

            public void Error(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
            {
            }

            public void Error(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
            {
            }

            public void Error(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
            {
            }

            public void Fatal(object message)
            {
            }

            public void Fatal(object message, Exception exception)
            {
            }

            public void FatalFormat(string format, params object[] args)
            {
            }

            public void FatalFormat(string format, Exception exception, params object[] args)
            {
            }

            public void FatalFormat(IFormatProvider formatProvider, string format, params object[] args)
            {
            }

            public void FatalFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
            {
            }

            public void Fatal(Action<FormatMessageHandler> formatMessageCallback)
            {
            }

            public void Fatal(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
            {
            }

            public void Fatal(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
            {
            }

            public void Fatal(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
            {
            }

            public bool IsTraceEnabled
            {
                get { return false; }
            }

            public bool IsDebugEnabled
            {
                get { return false; }
            }

            public bool IsErrorEnabled
            {
                get { return false; }
            }

            public bool IsFatalEnabled
            {
                get { return false; }
            }

            public bool IsInfoEnabled
            {
                get { return false; }
            }

            public bool IsWarnEnabled
            {
                get { return false; }
            }
        }
    }
}

#endif
