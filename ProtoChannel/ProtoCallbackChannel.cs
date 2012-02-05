using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ProtoChannel
{
    public class ProtoCallbackChannel : IProtoConnection
    {
        internal IProtoConnection Connection { get; set; }

        public uint SendStream(Stream stream, string streamName, string contentType)
        {
            return Connection.SendStream(stream, streamName, contentType);
        }

        public IAsyncResult BeginGetStream(uint streamId, AsyncCallback callback, object asyncState)
        {
            return Connection.BeginGetStream(streamId, callback, asyncState);
        }

        public ProtoStream EndGetStream(IAsyncResult asyncResult)
        {
            return Connection.EndGetStream(asyncResult);
        }

        public IAsyncResult BeginSendMessage(object message, Type responseType, AsyncCallback callback, object asyncState)
        {
            return Connection.BeginSendMessage(message, responseType, callback, asyncState);
        }

        public object EndSendMessage(IAsyncResult asyncResult)
        {
            return Connection.EndSendMessage(asyncResult);
        }

        public void PostMessage(object message)
        {
            Connection.PostMessage(message);
        }
    }
}
