using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using ProtoChannel.Util;

namespace ProtoChannel.Web
{
    internal abstract class Request : AsyncResultImpl
    {
        public HttpContext Context { get; private set; }

        protected Request(HttpContext context, AsyncCallback asyncCallback, object extraData)
            : base(asyncCallback, extraData)
        {
            Require.NotNull(context, "context");

            Context = context;
        }

        public static Request CreateRequest(HttpContext context, AsyncCallback asyncCallback, object extraData)
        {
            Require.NotNull(context, "context");

            switch (context.Request.Path)
            {
                case "/pchx/channel": return CreateChannelRequest(context, asyncCallback, extraData);
                case "/pchx/stream": return CreateStreamRequest(context, asyncCallback, extraData);
                default: return new InvalidRequest(context, asyncCallback, extraData, "Invalid request");
            }
        }

        private static Request CreateChannelRequest(HttpContext context, AsyncCallback asyncCallback, object extraData)
        {
            if (!ValidateVersion(context))
                return new InvalidRequest(context, asyncCallback, extraData, "Invalid VER query string parameter");

            switch (context.Request.HttpMethod)
            {
                case "GET": return CreateChannelGetRequest(context, asyncCallback, extraData);
                case "POST": return CreateChannelPostRequest(context, asyncCallback, extraData);
                default: return new InvalidRequest(context, asyncCallback, extraData, "Cannot process request method");
            }
        }

        private static bool ValidateVersion(HttpContext context)
        {
            string versionString = context.Request.QueryString["VER"];
            int version;

            return
                versionString != null &&
                int.TryParse(versionString, NumberStyles.None, CultureInfo.InvariantCulture, out version) &&
                version == ProtoClient.ProtocolVersion;
        }

        private static Request CreateChannelGetRequest(HttpContext context, AsyncCallback asyncCallback, object extraData)
        {
            if (context.Request.QueryString["CID"] != null)
            {
                var client = FindClient(context);

                if (client == null)
                    return new InvalidRequest(context, asyncCallback, extraData, "Invalid CID query string parameter");

                return new ChannelDownstreamRequest(context, asyncCallback, extraData, client);
            }
            else
            {
                string protocolVersionString = context.Request.QueryString["PVER"];
                int protocolVersion;

                if (
                    protocolVersionString == null ||
                    !int.TryParse(protocolVersionString, NumberStyles.None, CultureInfo.InvariantCulture, out protocolVersion) ||
                    protocolVersion < 0
                )
                    return new InvalidRequest(context, asyncCallback, extraData, "Invalid PVER query string parameter");

                return new ChannelCreateRequest(context, asyncCallback, extraData, protocolVersion);
            }
        }

        private static Request CreateChannelPostRequest(HttpContext context, AsyncCallback asyncCallback, object extraData)
        {
            var client = FindClient(context);

            if (client == null)
                return new InvalidRequest(context, asyncCallback, extraData, "Invalid CID query string parameter");

            return new ChannelUpstreamRequest(context, asyncCallback, extraData, client);
        }

        private static ProtoProxyClient FindClient(HttpContext context)
        {
            string channelId = context.Request.QueryString["CID"];

            if (channelId != null)
                return ProtoHandler.Proxy.FindClient(channelId);
            else
                return null;
        }

        private static Request CreateStreamRequest(HttpContext context, AsyncCallback asyncCallback, object extraData)
        {
            if (!ValidateVersion(context))
                return new InvalidRequest(context, asyncCallback, extraData, "Invalid VER query string parameter");

            string associationIdString = context.Request.QueryString["AID"];
            int? associationId = null;

            if (associationIdString != null)
            {
                int value;

                if (
                    int.TryParse(associationIdString, NumberStyles.None, CultureInfo.InvariantCulture, out value) &&
                    value >= 0
                )
                    associationId = value;
            }

            if (!associationId.HasValue && context.Request.HttpMethod != "POST")
                return new InvalidRequest(context, asyncCallback, extraData, "Invalid AID query string parameter");

            var client = FindClient(context);

            if (client == null)
                return new InvalidRequest(context, asyncCallback, extraData, "Invalid CID query string parameter");

            switch (context.Request.HttpMethod)
            {
                case "GET": return new StreamDownloadRequest(context, asyncCallback, extraData, client, associationId.Value);
                case "POST": return new StreamUploadRequest(context, asyncCallback, extraData, client, associationId);
                default: return new InvalidRequest(context, asyncCallback, extraData, "Cannot process request method");
            }
        }

        public virtual void EndRequest()
        {
            EndInvoke();
        }
    }
}
