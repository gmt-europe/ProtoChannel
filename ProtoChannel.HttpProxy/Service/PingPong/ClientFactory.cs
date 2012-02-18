using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProtoChannel.Web;

namespace ProtoChannel.HttpProxy.Service.PingPong
{
    public class ClientFactory : IProtoClientFactory
    {
        public ProtoClient CreateClient(string hostname, int port, int protocolVersion)
        {
            var configuration = new ClientConfiguration
            {
                CallbackObject = new ClientCallbackService(),
                ProtocolVersion = protocolVersion
            };

            return new ClientService(hostname, port, configuration);
        }
    }
}
