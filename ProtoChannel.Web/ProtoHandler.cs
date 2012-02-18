using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Configuration;

namespace ProtoChannel.Web
{
    public class ProtoHandler : IHttpAsyncHandler
    {
        internal static ProtoProxyHost Proxy { get; private set; }

        static ProtoHandler()
        {
            string clientFactoryTypeString = WebConfigurationManager.AppSettings["protochannel.client-factory"];

            if (clientFactoryTypeString == null)
                throw new InvalidOperationException("protochannel.client appSetting is missing");

            var clientFactoryType = Type.GetType(clientFactoryTypeString, true);

            string hostEndPoint = WebConfigurationManager.AppSettings["protochannel.host"];

            if (hostEndPoint == null)
                throw new InvalidOperationException("protochannel.host appSetting is missing");

            string[] parts = hostEndPoint.Split(new[] { ':' }, 2);
            string hostName = null;
            int hostPort = 0;

            if (parts.Length == 2)
            {
                int port;

                if (int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out port))
                {
                    hostName = parts[0];
                    hostPort = port;
                }
            }

            if (hostName == null)
                throw new InvalidOperationException("protochannel.host is of incorrect format; use <host>:<port>");

            Proxy = new ProtoProxyHost(hostName, hostPort, (IProtoClientFactory)Activator.CreateInstance(clientFactoryType));
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
