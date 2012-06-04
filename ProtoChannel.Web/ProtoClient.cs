using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoChannel.Web
{
    internal class ProtoClient : ProtoChannel.ProtoClient
    {
        [ThreadStatic]
        private static int? _currentProtocolVersion;

        private readonly int _protocolVersion;

        private ProtoClient(string hostname, int port, ProtoClientConfiguration configuration, int protocolVersion)
            : base(hostname, port, configuration)
        {
            _protocolVersion = protocolVersion;
        }

        public static ProtoClient CreateClient(string hostname, int port, ProtoClientConfiguration configuration, int protocolVersion)
        {
            // The protocol version may be read before our constructor completes.
            // Because of this we use a ThreadStatic here to alternatively
            // provide the protocol number.

            _currentProtocolVersion = protocolVersion;

            try
            {
                return new ProtoClient(hostname, port, configuration, protocolVersion);
            }
            finally
            {
                _currentProtocolVersion = null;
            }
        }

        protected internal override int ChooseProtocol(int minProtocol, int maxProtocol)
        {
            return _currentProtocolVersion.GetValueOrDefault(_protocolVersion);
        }
    }
}
