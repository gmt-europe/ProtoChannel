﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json;

namespace ProtoChannel.Web
{
    internal class ChannelDownstreamRequest : Request
    {
        private readonly ProtoProxyClient _client;

        public DateTime Created { get; private set; }

        public ChannelDownstreamRequest(HttpContext context, AsyncCallback asyncCallback, object extraData, ProtoProxyClient client)
            : base(context, asyncCallback, extraData)
        {
            Created = DateTime.Now;

            _client = client;

            _client.Touch();

            // Below content type is not gzipped by default. Gzipping the response
            // has the effect that chunked transport encoding doesn't work anymore.

            Context.Response.ContentType = "application/octet-stream";

            _client.AssignDownstream(this);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public void SendMessage(PendingDownstreamMessage message)
        {
            _client.Touch();

            using (var stringWriter = new StringWriter())
            {
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

                var response = stringWriter.GetStringBuilder().ToString();

                Context.Response.Write(String.Format("{0}\n", response.Length));
                Context.Response.Write(response);
                Context.Response.Flush();
            }
        }

        public override void EndRequest()
        {
            _client.Touch();

            base.EndRequest();

            // Message size of zero signifies end of the request.

            Context.Response.Write("0\n");
        }
    }
}