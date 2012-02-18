using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoChannel.Web
{
    internal class ProtoClient : ProtoChannel.ProtoClient
    {
        private readonly int _protocolVersion;

        public ProtoClient(string hostname, int port, ProtoClientConfiguration configuration, int protocolVersion)
            : base(hostname, port, configuration)
        {
            _protocolVersion = protocolVersion;
        }

        protected internal override int ChooseProtocol(int minProtocol, int maxProtocol)
        {
            return _protocolVersion;
        }
    }
}
