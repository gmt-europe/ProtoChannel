using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ProtoChannel.Demo.ProtoService;

namespace ProtoChannel.Demo
{
    internal class ProtoChannelClientRunner : TestClientRunner
    {
        public ProtoChannelClientRunner(IStatistics statistics, TestClientSettings settings)
            : base(statistics, settings)
        {
        }

        public override TestClient CreateClient()
        {
            return new ProtoChannelTestClient(Statistics, Settings);
        }

        private class ProtoChannelTestClient : TestClient
        {
            private readonly ClientService _service;
            private int _messagesSend;
            private readonly Stopwatch _stopwatch = new Stopwatch();

            public ProtoChannelTestClient(IStatistics statistics, TestClientSettings settings)
                : base(statistics, settings)
            {
                _service = new ClientService(settings.Host, Constants.ProtoChannelPort);
            }

            public override void Start()
            {
                SendMessage();
            }

            private void SendMessage()
            {
                _stopwatch.Reset();
                _stopwatch.Start();

                switch (Settings.MessageType)
                {
                    case ClientMessageType.Simple:
                        _service.BeginSimpleMessage(
                            new SimpleMessage { Value = _messagesSend },
                            BeginSimpleMessageCallback,
                            null
                        );
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }

            private void BeginSimpleMessageCallback(IAsyncResult asyncResult)
            {
                var result = _service.EndSimpleMessage(asyncResult);

                Debug.Assert(result.Value == _messagesSend);

                _stopwatch.Stop();

                Statistics.AddSendMessage(_stopwatch.ElapsedTicks);

                _messagesSend++;

                if (_messagesSend == Settings.RequestsPerClient)
                {
                    OnCompleted(EventArgs.Empty);

                    _service.Dispose();
                }
                else
                {
                    SendMessage();
                }
            }
        }
    }
}
