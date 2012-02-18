using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoChannel.Util;

namespace ProtoChannel.Web
{
    internal class ProtoProxyClient
    {
        private readonly object _syncRoot = new object();
        private readonly Queue<PendingDownstreamMessage> _messages = new Queue<PendingDownstreamMessage>();
        private ChannelDownstreamRequest _downstream;
        private uint _nextCallbackAssociationId;
        private readonly Dictionary<uint, AsyncResultImpl<object>> _pendingCallbacks = new Dictionary<uint, AsyncResultImpl<object>>();

        public string Key { get; private set; }

        public ProtoClient Client { get; private set; }

        public ChannelDownstreamRequest Downstream
        {
            get { return _downstream; }
            set
            {
                lock (_syncRoot)
                {
                    if (_downstream != null)
                        _downstream.SetAsCompleted(null, false);

                    _downstream = value;

                    if (_downstream != null)
                        SendMessages();
                }
            }
        }

        public ProtoProxyClient(string key, ProtoClient client)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            if (client == null)
                throw new ArgumentNullException("client");

            Key = key;
            Client = client;
        }

        public IAsyncResult BeginSendMessage(object message, AsyncCallback callback, object asyncState)
        {
            lock (_syncRoot)
            {
                var asyncMessage = new AsyncResultImpl<object>(callback, asyncState);

                uint associationId = _nextCallbackAssociationId++;

                _pendingCallbacks.Add(associationId, asyncMessage);

                _messages.Enqueue(new PendingDownstreamMessage(MessageKind.Request, associationId, message));

                if (Downstream != null)
                    SendMessages();

                return asyncMessage;
            }
        }

        public AsyncResultImpl<object> GetPendingCallbackMessage(uint associationId)
        {
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
            lock (_syncRoot)
            {
                return ((AsyncResultImpl<object>)asyncResult).EndInvoke();
            }
        }

        public void PostMessage(object message)
        {
            lock (_syncRoot)
            {
                _messages.Enqueue(new PendingDownstreamMessage(MessageKind.OneWay, 0, message));

                if (Downstream != null)
                    SendMessages();
            }
        }

        public void EndSendMessage(uint associationId, object message)
        {
            lock (_syncRoot)
            {
                _messages.Enqueue(new PendingDownstreamMessage(MessageKind.Response, associationId, message));

                if (Downstream != null)
                    SendMessages();
            }
        }

        private void SendMessages()
        {
            while (_messages.Count > 0)
            {
                try
                {
                    Downstream.SendMessage(_messages.Peek());

                    _messages.Dequeue();
                }
                catch
                {
                    return;
                }
            }
        }

        public void CheckDownstreamAge(TimeSpan maxAge)
        {
            lock (_syncRoot)
            {
                if (Downstream != null && Downstream.Created + maxAge < DateTime.Now)
                    Downstream = null;
            }
        }
    }
}
