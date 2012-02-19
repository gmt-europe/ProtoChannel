using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Configuration;

namespace ProtoChannel.Web
{
    public class ProtoHandler : IHttpAsyncHandler
    {
        internal static ProtoProxyHost Proxy { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations", Justification = "Cannot continue without configuration")]
        static ProtoHandler()
        {
            var config = (ProtoConfigurationSection)WebConfigurationManager.GetSection("protoChannel");

            if (config == null)
                throw new ProtoChannelException("No protoChannel configuration section has been provided");

            string hostName = config.Host;
            int hostPort = config.Port;

            string hostEndPoint = WebConfigurationManager.AppSettings["protochannel.host"];

            if (hostEndPoint != null)
            {
                string[] parts = hostEndPoint.Split(new[] { ':' }, 2);
                if (parts.Length == 2)
                {
                    int port;

                    if (int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out port))
                    {
                        hostName = parts[0];
                        hostPort = port;
                    }
                }
            }

            if (hostName == null || hostPort <= 0)
                throw new ProtoChannelException("No host and port have been provided; specify them either in the protoChannel config section or through the protochannel.host appSetting");

            Proxy = new ProtoProxyHost(hostName, hostPort, Assembly.Load(config.ServiceAssembly));
        }

        public void ProcessRequest(HttpContext context)
        {
            throw new NotSupportedException();
        }

        public bool IsReusable
        {
            get { return true; }
        }

        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            return Request.CreateRequest(context, cb, extraData);
        }

        public void EndProcessRequest(IAsyncResult result)
        {
            ((Request)result).EndRequest();
        }
    }
}
