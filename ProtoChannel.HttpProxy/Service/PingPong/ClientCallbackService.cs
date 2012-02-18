using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ProtoChannel.HttpProxy.Service.PingPong
{
    public class ClientCallbackService
    {
        public ManualResetEvent CallbackReceivedEvent { get; private set; }

        public ClientCallbackService()
        {
            CallbackReceivedEvent = new ManualResetEvent(false);
        }

        [ProtoMethod]
        public Pong Ping(Ping message)
        {
            CallbackReceivedEvent.Set();

            return new Pong { Payload = message.Payload };
        }

        [ProtoMethod(IsOneWay = true)]
        public void OneWayPing(OneWayPing message)
        {
            Console.WriteLine("One way ping received");

            CallbackReceivedEvent.Set();
        }
    }
}
