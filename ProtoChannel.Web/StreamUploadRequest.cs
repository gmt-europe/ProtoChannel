using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;

namespace ProtoChannel.Web
{
    internal class StreamUploadRequest : Request
    {
        private readonly ProtoProxyClient _client;
        private readonly int? _associationId;

        public StreamUploadRequest(HttpContext context, AsyncCallback asyncCallback, object extraData, ProtoProxyClient client, int? associationId)
            : base(context, asyncCallback, extraData)
        {
            Require.NotNull(client, "client");

            _client = client;
            _associationId = associationId;

            _client.Touch();

            HandleRequest();

            SetAsCompleted(null, true);
        }

        private void HandleRequest()
        {
            if (Context.Request.Files.Count > 1 && _associationId.HasValue)
                throw new HttpException("AID must be provided in request with multiple file uploads");

            for (int i = 0; i < Context.Request.Files.Count; i++)
            {
                var file = Context.Request.Files[i];

                string associationIdString = Context.Request.Form["AID_" + i];

                if (
                    associationIdString == null &&
                    (!_associationId.HasValue || Context.Request.Form.Count > 1)
                )
                    throw new HttpException("AID wasn't provided in the request");

                int associationId;

                if (_associationId.HasValue)
                    associationId = _associationId.Value;
                else if (!int.TryParse(associationIdString, out associationId))
                    throw new HttpException("Invalid AID");

                int responseId = _client.Client.SendStream(file.InputStream, file.FileName, file.ContentType, associationId);

                Debug.Assert(associationId == responseId);
            }
        }
    }
}
