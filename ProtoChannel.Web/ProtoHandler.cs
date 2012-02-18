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

        static ProtoHandler()
        {
            string serviceAssemblyString = WebConfigurationManager.AppSettings["protochannel.service-assembly"];

            if (serviceAssemblyString == null)
                throw new InvalidOperationException("protochannel.service-assembly appSetting is missing");

            var serviceAssembly = Assembly.Load(serviceAssemblyString);

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

            Proxy = new ProtoProxyHost(hostName, hostPort, serviceAssembly);
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
