using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using ProtoChannel.Demo.Shared;

namespace ProtoChannel.Demo.Wcf
{
    [ServiceContract]
    public class ServerService
    {
        [OperationContract]
        public SimpleMessage SimpleMessage(SimpleMessage message)
        {
            return message;
        }

        [OperationContract]
        public ComplexMessage ComplexMessage(ComplexMessage message)
        {
            return message;
        }

        [OperationContract]
        public void ReceiveStream(Stream stream)
        {
            var buffer = new byte[0x1000];

            while (stream.Read(buffer, 0, buffer.Length) > 0)
            {
            }
        }
    }
}
