using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ProtoChannel
{
    public interface IProtoConnection
    {
        uint SendStream(Stream stream, string streamName, string contentType);

        IAsyncResult BeginGetStream(uint streamId, AsyncCallback callback, object asyncState);

        ProtoStream EndGetStream(IAsyncResult asyncResult);

        IAsyncResult BeginSendMessage(object message, Type responseType, AsyncCallback callback, object asyncState);

        object EndSendMessage(IAsyncResult asyncResult);

        void PostMessage(object message);
    }
}
