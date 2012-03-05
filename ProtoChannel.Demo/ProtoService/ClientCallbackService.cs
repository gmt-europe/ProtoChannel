using System;
using System.Collections.Generic;
using System.Text;
using ProtoChannel.Demo.Shared;

namespace ProtoChannel.Demo.ProtoService
{
    public class ClientCallbackService
    {
        public event EventHandler StreamReceived;

        protected virtual void OnStreamReceived(EventArgs e)
        {
            var ev = StreamReceived;

            if (ev != null)
                ev(this, e);
        }

        [ProtoMethod(IsOneWay = true)]
        public void StreamReceivedMessage(StreamReceivedMessage message)
        {
            OnStreamReceived(EventArgs.Empty);
        }
    }
}
