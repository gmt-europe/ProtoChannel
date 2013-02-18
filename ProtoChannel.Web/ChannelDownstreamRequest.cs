extern alias JSON;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using JSON::Newtonsoft.Json;

namespace ProtoChannel.Web
{
    internal class ChannelDownstreamRequest : Request
    {
        private static readonly string IframePadding = new string(' ', 1024);

        private readonly ProtoProxyClient _client;
        private readonly int? _downstreamId;

        public DateTime Created { get; private set; }

        public ChannelDownstreamRequest(HttpContext context, AsyncCallback asyncCallback, object extraData, ProtoProxyClient client)
            : base(context, asyncCallback, extraData)
        {
            Created = DateTime.Now;

            _client = client;

            _client.Touch();

            string downstreamIdString = Context.Request.QueryString["DID"];

            if (downstreamIdString != null)
            {
                int downstreamId;

                if (!int.TryParse(downstreamIdString, NumberStyles.None, CultureInfo.InvariantCulture, out downstreamId))
                    throw new ProtoChannelException("Invalid DID query string parameter");

                _downstreamId = downstreamId;
            }

            if (_downstreamId.HasValue)
            {
                // For IE, we communicate through framed JSON. This requires
                // us to set the content type to text/html. Ensure that dynamic
                // compression is disabled in web.config.

                Context.Response.ContentType = "text/html";

                // Send 1K in padding.

                Context.Response.Write(IframePadding);
                Context.Response.Flush();
            }
            else
            {
                // Below content type is not gzipped by default. Gzipping the response
                // has the effect that chunked transport encoding doesn't work anymore.

                Context.Response.ContentType = "application/octet-stream";
            }

            _client.AssignDownstream(this);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public void SendMessage(PendingDownstreamMessage message)
        {
            _client.Touch();

            if (_downstreamId.HasValue)
            {
                var sb = new StringBuilder();

                sb.Append("<script>parent.__mr('");
                sb.Append(_client.Key);
                sb.Append("',");
                sb.Append(_downstreamId.Value);
                sb.Append(",");

                BuildResponseJson(sb, message);

                sb.AppendLine(");</script>");

                Context.Response.Write(sb.ToString());
                Context.Response.Flush();
            }
            else
            {
                var sb = new StringBuilder();

                BuildResponseJson(sb, message);

                string response = sb.ToString();

                Context.Response.Write(String.Format("{0}\n", response.Length));
                Context.Response.Write(response);
                Context.Response.Flush();
            }
        }

        private void BuildResponseJson(StringBuilder sb, PendingDownstreamMessage message)
        {
            using (var stringWriter = new StringWriter(sb))
            using (var writer = new JsonTextWriter(stringWriter))
            {
                if (message == null)
                {
                    writer.WriteStartArray();
                    writer.WriteValue("noop");
                    writer.WriteEndArray();
                }
                else
                {
                    var responseType = message.Message.GetType();
                    var responseServiceType = ServiceRegistry.GetAssembly(responseType.Assembly).TypesByType[responseType];

                    writer.WriteStartObject();
                    writer.WritePropertyName("r");
                    writer.WriteValue((int)message.Kind);
                    writer.WritePropertyName("a");
                    writer.WriteValue((int)message.AssociationId);
                    writer.WritePropertyName("t");
                    writer.WriteValue(responseServiceType.Message.Id);
                    writer.WritePropertyName("p");

                    JsonUtil.SerializeMessage(writer, responseServiceType, message.Message);

                    writer.WriteEndObject();
                }
            }
        }

        public override void EndRequest()
        {
            _client.Touch();

            base.EndRequest();

            if (_downstreamId.HasValue)
            {
                var sb = new StringBuilder();

                sb.Append("<script>parent.__mr('");
                sb.Append(_client.Key);
                sb.Append("',");
                sb.Append(_downstreamId.Value);
                sb.Append(",null);</script>");

                Context.Response.Write(sb.ToString());
            }
            else
            {
                // Message size of zero signifies end of the request.

                Context.Response.Write("0\n");
            }
        }
    }
}
