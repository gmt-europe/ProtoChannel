using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoChannel.Test.Services.PingPong
{
    internal class ServerService
    {
        [ProtoMethod]
        public Pong Ping(Ping message)
        {
            Console.WriteLine("Ping received");

            return new Pong { Payload = message.Payload };
        }
    }
}
