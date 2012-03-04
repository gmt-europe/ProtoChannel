using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace ProtoChannel.Demo.ProtoService
{
    public class ServerService
    {
        [ProtoMethod]
        public SimpleMessage SimpleMessage(SimpleMessage message)
        {
            return message;
        }
    }
}
