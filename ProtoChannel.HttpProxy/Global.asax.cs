using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.SessionState;
using ProtoChannel.Test.Service;
using ProtoChannel.Web;

namespace ProtoChannel.HttpProxy
{
    public class Global : HttpApplication
    {
        private ProtoHost<ServerService> _host;

        protected void Application_Start(object sender, EventArgs e)
        {
            _host = new ProtoHost<ServerService>(new IPEndPoint(IPAddress.Loopback, 0), new ProtoHostConfiguration
            {
                MinimumProtocolNumber = 1,
                MaximumProtocolNumber = 1
            });

            WebConfigurationManager.AppSettings["protochannel.host"] = _host.LocalEndPoint.ToString();
        }

        protected void Application_End(object sender, EventArgs e)
        {
            if (_host != null)
            {
                _host.Close(CloseMode.Abort);
                _host.Dispose();
            }
        }
    }
}