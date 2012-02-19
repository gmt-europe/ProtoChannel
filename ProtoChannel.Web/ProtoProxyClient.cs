using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoChannel.Util;

namespace ProtoChannel.Web
{
    internal class ProtoProxyClient : IDisposable
    {
        private readonly ProtoProxyHost _host;
        private static readonly TimeSpan DownstreamMaxAge = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan LastInteractionMaxAge = TimeSpan.FromMinutes(1);

        private bool _disposed;
        private readonly object _syncRoot = new object();
        private readonly Queue<PendingDownstreamMessage> _messages = new Queue<PendingDownstreamMessage>();
        private ChannelDownstreamRequest _downstream;
        private uint _nextCallbackAssociationId;
        private readonly Dictionary<uint, AsyncResultImpl<object>> _pendingCallbacks = new Dictionary<uint, AsyncResultImpl<object>>();
        private DateTime _lastInteraction;

        public string Key { get; private set; }

        public ProtoClient Client { get; private set; }

        public ProtoProxyClient(ProtoProxyHost host, string key, ProtoClient client)
        {
            Require.NotNull(host, "host");
            Require.NotNull(key, "key");
            Require.NotNull(client, "client");

            _host = host;
            Key = key;
            Client = client;

            _lastInteraction = DateTime.Now;
        }

        public void AssignDownstream(ChannelDownstreamRequest downstream)
        {
            VerifyNotDisposed();

            Require.NotNull(downstream, "downstream");

            lock (_syncRoot)
            {
                DetachDownstream();

                _downstream = downstream;

                SendMessages();
            }
        }

        private void DetachDownstream()
        {
            if (_downstream != null)
            {
                _downstream.SetAsCompleted(null, false);
                _downstream = null;
            }
        }

        private ChannelDownstreamRequest GetDownstream()
        {
            if (_downstream == null)
                return null;

            if (_downstream.Created + DownstreamMaxAge < DateTime.Now || !_downstream.Context.Response.IsClientConnected)
                DetachDownstream();

            return _downstream;
        }

        public IAsyncResult BeginSendMessage(object message, AsyncCallback callback, object asyncState)
        {
            VerifyNotDisposed();

            lock (_syncRoot)
            {
                var asyncMessage = new AsyncResultImpl<object>(callback, asyncState);

                uint associationId = _nextCallbackAssociationId++;

                _pendingCallbacks.Add(associationId, asyncMessage);

                _messages.Enqueue(new PendingDownstreamMessage(MessageKind.Request, associationId, message));

                SendMessages();

                return asyncMessage;
            }
        }

        public AsyncResultImpl<object> GetPendingCallbackMessage(uint associationId)
        {
            VerifyNotDisposed();

            lock (_syncRoot)
            {
                AsyncResultImpl<object> result;

                if (!_pendingCallbacks.TryGetValue(associationId, out result))
                    throw new ProtoChannelException("No pending callback message of the requested ID is available");

                _pendingCallbacks.Remove(associationId);

                return result;
            }
        }

        public object EndSendMessage(IAsyncResult asyncResult)
        {
            VerifyNotDisposed();

            lock (_syncRoot)
            {
                return ((AsyncResultImpl<object>)asyncResult).EndInvoke();
            }
        }

        public void PostMessage(object message)
        {
            VerifyNotDisposed();

            lock (_syncRoot)
            {
                _messages.Enqueue(new PendingDownstreamMessage(MessageKind.OneWay, 0, message));

                SendMessages();
            }
        }

        public void EndSendMessage(uint associationId, object message)
        {
            VerifyNotDisposed();

            lock (_syncRoot)
            {
                _messages.Enqueue(new PendingDownstreamMessage(MessageKind.Response, associationId, message));

                SendMessages();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void SendMessages()
        {
            while (_messages.Count > 0)
            {
                try
                {
                    var downstream = GetDownstream();

                    if (downstream != null)
                        downstream.SendMessage(_messages.Peek());

                    _messages.Dequeue();
                }
                catch
                {
                    DetachDownstream();

                    return;
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void Maintenance()
        {
            VerifyNotDisposed();

            lock (_syncRoot)
            {
                if (_lastInteraction + LastInteractionMaxAge < DateTime.Now)
                {
                    Dispose();
                }
                else
                {
                    try
                    {
                        var downstream = GetDownstream();

                        if (downstream != null)
                        {
                            // Send a no-op.

                            downstream.SendMessage(null);
                        }
                    }
                    catch
                    {
                        DetachDownstream();
                    }
                }
            }
        }

        public void Touch()
        {
            lock (_syncRoot)
            {
                _lastInteraction = DateTime.Now;
            }
        }

        private void VerifyNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        public void Dispose()
        {
            lock (_syncRoot)
            {
                if (!_disposed)
                {
                    _host.RemoveClient(this);

                    DetachDownstream();

                    Client.Dispose();

                    _disposed = true;
                }
            }
        }
    }
}
