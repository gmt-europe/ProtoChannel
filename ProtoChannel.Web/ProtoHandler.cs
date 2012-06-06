using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Compression;
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

        public static ProtoChannel.ProtoClient FindClient(string channelId)
        {
            if (channelId == null)
                throw new ArgumentNullException("channelId");

            var client = Proxy.FindClient(channelId);

            if (client != null)
                return client.Client;

            return null;
        }

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

                    if (
                        int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out port) &&
                        port > 0
                    )
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

        public static void BeginRequest()
        {
            if (
                HttpContext.Current.Request.Path.EndsWith("/pchx/channel") &&
                HttpContext.Current.Request.HttpMethod == "GET")
            {
                // Disable GZip compression for the channel.

                string acceptEncoding = HttpContext.Current.Request.Headers["Accept-Encoding"];

                if (acceptEncoding != null)
                {
                    var sb = new StringBuilder();

                    foreach (string part in acceptEncoding.Split(','))
                    {
                        switch (part.Trim().ToLowerInvariant())
                        {
                            case "gzip":
                            case "deflate":
                                break;

                            default:
                                if (sb.Length > 0)
                                    sb.Append(',');
                                sb.Append(part);
                                break;
                        }
                    }

                    HttpContext.Current.Request.Headers["Accept-Encoding"] = sb.ToString();
                }
            }
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
