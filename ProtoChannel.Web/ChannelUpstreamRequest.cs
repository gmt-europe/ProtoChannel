using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json;

namespace ProtoChannel.Web
{
    internal class ChannelUpstreamRequest : Request
    {
        private readonly ProtoProxyClient _client;

        public ChannelUpstreamRequest(HttpContext context, AsyncCallback asyncCallback, object extraData, ProtoProxyClient client)
            : base(context, asyncCallback, extraData)
        {
            _client = client;

            _client.Touch();

            HandleRequest();
            
            SetAsCompleted(null, true);
        }

        private void HandleRequest()
        {
            string countString = Context.Request.Form["count"];

            if (countString == null)
                throw new HttpException("count form parameter missing");

            int count;

            if (
                !int.TryParse(countString, NumberStyles.None, CultureInfo.InvariantCulture, out count) ||
                count < 1
            )
                throw new HttpException("count form parameter invalid");

            for (int i = 0; i < count; i++)
            {
                string request = Context.Request.Form["req" + i.ToString(CultureInfo.InvariantCulture) + "_key"];

                if (request == null)
                    throw new HttpException("Missing request " + i.ToString(CultureInfo.InvariantCulture));

                ParseRequest(request);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        private void ParseRequest(string request)
        {
            using (var stringReader = new StringReader(request))
            using (var reader = new JsonTextReader(stringReader))
            {
                reader.DateParseHandling = DateParseHandling.None;

                // Messages parsed as a dictionary with the following
                // parameters:
                //
                //   'r': Request type: 0, 1 or 2
                //   'a': Association ID when the request type is 1 or 2
                //   't': Message type
                //   'p': Payload

                if (!reader.Read())
                    throw new HttpException("Invalid request");

                if (reader.TokenType == JsonToken.StartObject)
                {
                    ProcessMessageRequest(reader);
                }
                else if (reader.TokenType == JsonToken.StartArray)
                {
                    ReadToken(reader, JsonToken.String);

                    string action = (string)reader.Value;

                    ReadToken(reader, JsonToken.EndArray);

                    switch (action)
                    {
                        case "close":
                            _client.Dispose();
                            break;

                        default:
                            throw new HttpException("Invalid request");
                    }
                }
                else
                {
                    throw new HttpException("Invalid request");
                }

                if (reader.Read())
                    throw new HttpException("Invalid request");

            }
        }

        private void ProcessMessageRequest(JsonTextReader reader)
        {
            MessageKind? messageKind = null;
            int? associationId = null;
            int? messageType = null;
            bool hadPayload = false;

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                    break;
                else if (reader.TokenType != JsonToken.PropertyName)
                    throw new HttpException("Invalid request");

                switch ((string)reader.Value)
                {
                    case "r":
                        ReadToken(reader, JsonToken.Integer);
                        int value = (int)(long)reader.Value;

                        if (value < 0 || value > 2)
                            throw new HttpException("Invalid request type");

                        messageKind = (MessageKind)value;
                        break;

                    case "a":
                        ReadToken(reader, JsonToken.Integer);
                        associationId = (int)(long)reader.Value;

                        if (associationId < 0)
                            throw new HttpException("Invalid association id");
                        break;

                    case "t":
                        ReadToken(reader, JsonToken.Integer);
                        messageType = (int)(long)reader.Value;

                        if (messageType < 0)
                            throw new HttpException("Invalid message type");
                        break;

                    case "p":
                        hadPayload = true;

                        if (
                            !messageKind.HasValue ||
                                !messageType.HasValue ||
                                    (messageKind.Value != MessageKind.OneWay && !associationId.HasValue)
                            )
                            throw new HttpException("Invalid request");

                        ProcessRequest(
                            messageKind.Value,
                            messageType.Value,
                            (uint)associationId.GetValueOrDefault(0),
                            reader
                            );
                        break;

                    default:
                        throw new HttpException("Invalid request");
                }
            }

            if (!hadPayload)
                throw new HttpException("Invalid request");
        }

        private void ProcessRequest(MessageKind messageKind, int messageType, uint associationId, JsonTextReader reader)
        {
            var serviceType = ServiceRegistry.GetAssembly(_client.Client.ServiceAssembly.Assembly).TypesById[messageType];

            var message = JsonUtil.DeserializeMessage(reader, serviceType);

            switch (messageKind)
            {
                case MessageKind.Request:
                    ProtoProxyHost.BeginSendMessage(_client, message, associationId);
                    break;

                case MessageKind.OneWay:
                    ProtoProxyHost.PostMessage(_client, message);
                    break;

                case MessageKind.Response:
                    var pendingCallback = _client.GetPendingCallbackMessage(associationId);

                    pendingCallback.SetAsCompleted(message, false);
                    break;
            }
        }

        private static void ReadToken(JsonTextReader reader, JsonToken token)
        {
            if (!reader.Read() || reader.TokenType != token)
                throw new HttpException("Invalid request");
        }

        public override void EndRequest()
        {
            base.EndRequest();

            Context.Response.ContentType = "text/plain";
            Context.Response.Write("ok");
        }
    }
}
