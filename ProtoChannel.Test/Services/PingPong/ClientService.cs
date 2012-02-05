using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace ProtoChannel.Test.Services.PingPong
{
    internal class ClientService : ProtoClient
    {
        public ClientService(IPEndPoint remoteEndPoint)
            : base(remoteEndPoint)
        {
        }

        public ClientService(IPEndPoint remoteEndPoint, ProtoClientConfiguration configuration)
            : base(remoteEndPoint, configuration)
        {
        }

        public Pong Ping(Ping message)
        {
            return SendMessage<Pong>(message);
        }

        public IAsyncResult BeginPing(Ping message, AsyncCallback callback, object asyncState)
        {
            return BeginSendMessage(message, typeof(Pong), callback, asyncState);
        }

        public Pong EndPing(IAsyncResult asyncResult)
        {
            return EndSendMessage<Pong>(asyncResult);
        }

        public void OneWayPing(OneWayPing message)
        {
            PostMessage(message);
        }
    }
}
