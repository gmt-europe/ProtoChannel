using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ProtoChannel.CodeGenerator
{
    [Serializable]
    internal class CommandLineArgumentException : Exception
    {
        public CommandLineArgumentException()
        {
        }

        public CommandLineArgumentException(string message)
            : base(message)
        {
        }

        public CommandLineArgumentException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected CommandLineArgumentException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
