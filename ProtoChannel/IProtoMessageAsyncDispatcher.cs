using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoChannel
{
    internal interface IProtoMessageAsyncDispatcher
    {
        IAsyncResult BeginDispatch(object message, AsyncCallback callback, object asyncState);
        object EndDispatch(IAsyncResult asyncResult);
        void DispatchPost(object message);
    }
}
