using System;
using System.Collections.Generic;
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
    }
}
