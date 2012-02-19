using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace ProtoChannel.Web
{
    internal class StreamDownloadRequest : Request
    {
        private readonly ProtoProxyClient _client;
        private readonly uint _associationId;

        public StreamDownloadRequest(HttpContext context, AsyncCallback asyncCallback, object extraData, ProtoProxyClient client, uint associationId)
            : base(context, asyncCallback, extraData)
        {
            Require.NotNull(client, "client");

            _client = client;
            _associationId = associationId;

            _client.Touch();

            HandleRequest();
        }

        private void HandleRequest()
        {
            _client.Client.BeginGetStream(_associationId, BeginGetStreamCallback, null);
        }

        private void BeginGetStreamCallback(IAsyncResult asyncResult)
        {
            var stream = _client.Client.EndGetStream(asyncResult);

            _client.Touch();

            string disposition = Context.Request.QueryString["disposition"] ?? "inline";

            Context.Response.ContentType = stream.ContentType;
            Context.Response.Headers["Content-Disposition"] = disposition + "; filename=" + stream.StreamName;

            stream.Stream.CopyTo(Context.Response.OutputStream);

            SetAsCompleted(null, false);
        }
    }
}
