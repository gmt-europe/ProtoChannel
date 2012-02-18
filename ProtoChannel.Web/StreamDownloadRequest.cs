using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace ProtoChannel.Web
{
    internal class StreamDownloadRequest : Request
    {
        public StreamDownloadRequest(HttpContext context, AsyncCallback asyncCallback, object extraData, ProtoProxyClient client, int associationId)
            : base(context, asyncCallback, extraData)
        {
        }
    }
}
