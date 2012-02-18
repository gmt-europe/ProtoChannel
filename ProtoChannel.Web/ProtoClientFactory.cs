using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoChannel.Web
{
    public interface IProtoClientFactory
    {
        ProtoClient CreateClient(string hostname, int port, int protocolVersion);
    }
}
