using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoChannel.Web
{
    internal class CallbackChannel : IProtoMessageDispatcher
    {
        private readonly ProtoProxyHost _host;
        private readonly string _channelId;
        private ProtoProxyClient _client;

        public CallbackChannel(ProtoProxyHost host, string channelId)
        {
            if (host == null)
                throw new ArgumentNullException("host");
            if (channelId == null)
                throw new ArgumentNullException("channelId");

            _host = host;
            _channelId = channelId;
        }

        private ProtoProxyClient Client
        {
            get
            {
                if (_client == null)
                    _client = _host.FindClient(_channelId);

                return _client;
            }
        }

        public IAsyncResult BeginDispatch(object message, AsyncCallback callback, object asyncState)
        {
            return Client.BeginSendMessage(message, callback, asyncState);
        }

        public object EndDispatch(IAsyncResult asyncResult)
        {
            return Client.EndSendMessage(asyncResult);
        }

        public void DispatchPost(object message)
        {
            Client.PostMessage(message);
        }
    }
}
