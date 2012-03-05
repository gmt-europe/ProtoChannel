using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using ProtoChannel.Demo.Shared;

namespace ProtoChannel.Demo.ProtoService
{
    [ProtoCallbackContract(typeof(ServerCallbackService))]
    public class ServerService
    {
        [ProtoMethod]
        public SimpleMessage SimpleMessage(SimpleMessage message)
        {
            return message;
        }

        [ProtoMethod]
        public ComplexMessage ComplexMessage(ComplexMessage message)
        {
            return message;
        }

        [ProtoMethod(IsOneWay = true)]
        public void StreamMessage(StreamMessage message)
        {
            var callbackService = OperationContext.Current.GetCallbackChannel<ServerCallbackService>();

            var pendingStream = new PendingGetStream(callbackService);

            callbackService.BeginGetStream((int)message.StreamId, pendingStream.BeginGetStreamCallback, null);
        }

        private class PendingGetStream
        {
            private readonly ServerCallbackService _callbackService;

            public PendingGetStream(ServerCallbackService callbackService)
            {
                _callbackService = callbackService;
            }

            public void BeginGetStreamCallback(IAsyncResult asyncResult)
            {
                using (_callbackService.EndGetStream(asyncResult))
                {
                }

                _callbackService.StreamReceivedMessage(new StreamReceivedMessage());
            }
        }
    }
}
