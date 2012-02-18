using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace ProtoChannel.Web
{
    internal class InvalidRequest : Request
    {
        private readonly string _error;

        public InvalidRequest(HttpContext context, AsyncCallback asyncCallback, object extraData, string error)
            : base(context, asyncCallback, extraData)
        {
            if (error == null)
                throw new ArgumentNullException("error");

            _error = error;

            SetAsCompleted(null, true);
        }

        public override void EndRequest()
        {
            base.EndRequest();

            Context.Response.Status = "500 " + _error;
            Context.Response.Write("<h1>" + Context.Server.HtmlEncode(_error) + "</h1>");
        }
    }
}
