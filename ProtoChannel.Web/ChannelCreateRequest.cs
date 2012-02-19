using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace ProtoChannel.Web
{
    internal class ChannelCreateRequest : Request
    {
        private readonly int _protocolVersion;
        private string _channelId;

        public ChannelCreateRequest(HttpContext context, AsyncCallback asyncCallback, object extraData, int protocolVersion)
            : base(context, asyncCallback, extraData)
        {
            _protocolVersion = protocolVersion;

            Task.Factory.StartNew(Execute);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void Execute()
        {
            try
            {
                _channelId = ProtoHandler.Proxy.CreateClient(_protocolVersion);

                SetAsCompleted(null, false);
            }
            catch (Exception ex)
            {
                SetAsCompleted(ex, false);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public override void EndRequest()
        {
            base.EndRequest();

            Context.Response.ContentType = "application/json";
            
            using (var streamWriter = new StreamWriter(Context.Response.OutputStream))
            using (var writer = new JsonTextWriter(streamWriter))
            {
                writer.WriteStartObject();
                writer.WritePropertyName("c");
                writer.WriteValue(_channelId);
                writer.WriteEndObject();
            }
        }
    }
}
