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

        public StreamResponse RequestStream(StreamRequest message)
        {
            return SendMessage<StreamResponse>(message);
        }

        public IAsyncResult BeginRequestStream(StreamRequest message, AsyncCallback callback, object asyncState)
        {
            return BeginSendMessage(message, typeof(StreamResponse), callback, asyncState);
        }

        public StreamResponse EndRequestStream(IAsyncResult asyncResult)
        {
            return EndSendMessage<StreamResponse>(asyncResult);
        }
    }
}
