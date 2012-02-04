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

        public IAsyncResult BeginPing(Ping message)
        {
            return BeginSendMessage(message);
        }

        public Pong EndPing(IAsyncResult asyncResult)
        {
            return EndSendMessage<Pong>(asyncResult);
        }
    }
}
