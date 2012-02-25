using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ProtoChannel.Test.Service
{
    [ProtoCallbackContract(typeof(ServerCallbackService))]
    public class ServerService
    {
        [ProtoMethod]
        public Pong Ping(Ping message)
        {
            return new Pong { Payload = message.Payload };
        }

        [ProtoMethod(IsOneWay = true)]
        public void OneWayPing(OneWayPing message)
        {
            Console.WriteLine("One way ping received");

            OperationContext.Current.GetCallbackChannel<ServerCallbackService>().OneWayPing(new OneWayPing
            {
                Payload = message.Payload
            });
        }

        [ProtoMethod(IsOneWay = true)]
        public void StreamUpload(StreamResponse stream)
        {
            Console.WriteLine("Stream response received");

            var callbackService = OperationContext.Current.GetCallbackChannel<ServerCallbackService>();

            var pendingStream = new PendingGetStream(callbackService);

            callbackService.BeginGetStream((int)stream.StreamId, pendingStream.BeginGetStreamCallback, null);
        }

        [ProtoMethod]
        public StreamResponse StreamRequest(StreamRequest request)
        {
            Console.WriteLine("Stream request received");

            byte[] payload = Encoding.UTF8.GetBytes("This is a stream payload");

            var stream = new MemoryStream(payload);

            int aid = OperationContext.Current.GetCallbackChannel<ServerCallbackService>().SendStream(
                stream, "Stream request.txt", "text/plain"
            );

            return new StreamResponse
            {
                StreamId = (uint)aid
            };
        }

        [ProtoMethod]
        public DefaultValueTests DefaultValueTest(DefaultValueTests message)
        {
            return message;
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
                var stream = _callbackService.EndGetStream(asyncResult);

                _callbackService.OneWayPing(new OneWayPing
                {
                    Payload = String.Format("Received stream: {0}, {1}, {2}", stream.StreamName, stream.Length, stream.ContentType)
                });
            }
        }
    }
}
