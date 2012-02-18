using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace ProtoChannel.HttpProxy.Service.PingPong
{
    public class ClientService : ProtoClient
    {
        public ClientService(IPEndPoint remoteEndPoint)
            : base(remoteEndPoint)
        {
        }

        public ClientService(IPEndPoint remoteEndPoint, ProtoClientConfiguration configuration)
            : base(remoteEndPoint, configuration)
        {
        }

        public ClientService(IPAddress address, int port)
            : base(address, port)
        {
        }

        public ClientService(IPAddress address, int port, ProtoClientConfiguration configuration)
            : base(address, port, configuration)
        {
        }

        public ClientService(string hostname, int port)
            : base(hostname, port)
        {
        }

        public ClientService(string hostname, int port, ProtoClientConfiguration configuration)
            : base(hostname, port, configuration)
        {
        }

        protected override int ChooseProtocol(int minProtocol, int maxProtocol)
        {
            return ((ClientConfiguration)Configuration).ProtocolVersion;
        }

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
