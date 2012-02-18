using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoChannel.HttpProxy.Service.PingPong
{
    public class ServerCallbackService : ProtoCallbackChannel
    {
        public Pong Ping(Ping message)
        {
            return EndPing(BeginPing(message, null, null));
        }

        public IAsyncResult BeginPing(Ping message, AsyncCallback callback, object asyncState)
        {
            return BeginSendMessage(message, typeof(Pong), callback, asyncState);
        }

        public Pong EndPing(IAsyncResult asyncResult)
        {
            return (Pong)EndSendMessage(asyncResult);
        }

        public void OneWayPing(OneWayPing message)
        {
            PostMessage(message);
        }
    }
}
