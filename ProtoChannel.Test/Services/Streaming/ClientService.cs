using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace ProtoChannel.Test.Services.Streaming
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

        public StreamResponse RequestStream(StreamRequest message)
        {
            return (StreamResponse)EndSendMessage(BeginSendMessage(message, typeof(StreamResponse), null, null));
        }

        public IAsyncResult BeginRequestStream(StreamRequest message, AsyncCallback callback, object asyncState)
        {
            return BeginSendMessage(message, typeof(StreamResponse), callback, asyncState);
        }

        public StreamResponse EndRequestStream(IAsyncResult asyncResult)
        {
            return (StreamResponse)EndSendMessage(asyncResult);
        }
    }
}
