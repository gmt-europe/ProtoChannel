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

        ProtoStream GetStream(uint streamId);

        IAsyncResult BeginGetStream(uint streamId, AsyncCallback callback, object asyncState);

        ProtoStream EndGetStream(IAsyncResult asyncResult);
    }
}
