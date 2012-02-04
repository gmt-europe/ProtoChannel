using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ProtoChannel
{
    [Serializable]
    public class ProtoChannelException : Exception
    {
        public ProtoChannelException()
        {
        }

        public ProtoChannelException(string message)
            : base(message)
        {
        }

        public ProtoChannelException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ProtoChannelException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
