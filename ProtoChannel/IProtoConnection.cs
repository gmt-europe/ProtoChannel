using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ProtoChannel
{
    public interface IProtoConnection
    {
        int SendStream(Stream stream, string streamName, string contentType);

        int SendStream(Stream stream, string streamName, string contentType, StreamDisposition disposition);

        ProtoStream GetStream(int streamId);

        IAsyncResult BeginGetStream(int streamId, AsyncCallback callback, object asyncState);

        ProtoStream EndGetStream(IAsyncResult asyncResult);

        IAsyncResult BeginSendMessage(object message, Type responseType, AsyncCallback callback, object asyncState);

        object EndSendMessage(IAsyncResult asyncResult);

        void PostMessage(object message);
    }
}
