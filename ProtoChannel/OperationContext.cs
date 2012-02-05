using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ProtoChannel
{
    public class OperationContext
    {
        private readonly ProtoCallbackChannel _callbackChannel;

        [ThreadStatic]
        private static OperationContext _current;

        public IProtoConnection Connection { get; private set; }

        public static OperationContext Current
        {
            get { return _current; }
        }

        internal OperationContext(IProtoConnection connection, ProtoCallbackChannel callbackChannel)
        {
            _callbackChannel = callbackChannel;
            if (connection == null)
                throw new ArgumentNullException("connection");

            Connection = connection;
        }

        public T GetCallbackChannel<T>()
            where T : ProtoCallbackChannel
        {
            if (_callbackChannel == null || !(_callbackChannel is T))
                throw new ProtoChannelException("Callback channel not available");

            return (T)_callbackChannel;
        }

        internal static IDisposable SetScope(OperationContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            var current = _current;

            _current = context;

            return new ScopeRestorer(current);
        }

        private class ScopeRestorer : IDisposable
        {
            private readonly OperationContext _previousContext;
            private bool _disposed;

            public ScopeRestorer(OperationContext previousContext)
            {
                _previousContext = previousContext;
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _current = _previousContext;

                    _disposed = true;
                }
            }
        }
    }
}
