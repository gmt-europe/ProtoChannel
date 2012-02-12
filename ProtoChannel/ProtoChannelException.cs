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

        public ProtoChannelException(ProtocolError error)
            : base(String.Format("Protocol error {0}", error))
        {
            Error = error;
        }

        public ProtocolError? Error { get; private set; }

        protected ProtoChannelException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Error = (ProtocolError?)info.GetValue("Error", typeof(ProtocolError?));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Error", Error);

            base.GetObjectData(info, context);
        }
    }
}
