using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoChannel.Web
{
    internal class ProtoProxyClient
    {
        private readonly object _syncRoot = new object();
        private readonly Queue<PendingDownstreamMessage> _messages = new Queue<PendingDownstreamMessage>();
        private ChannelDownstreamRequest _downstream;

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

        public void EndSendMessage(uint associationId, object message)
        {
            lock (_syncRoot)
            {
                _messages.Enqueue(new PendingDownstreamMessage(associationId, message));

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
    }
}
