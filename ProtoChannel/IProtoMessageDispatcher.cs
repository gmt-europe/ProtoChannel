using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoChannel
{
    internal interface IProtoMessageDispatcher
    {
        object Dispatch(object message);
    }
}
