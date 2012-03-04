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
            private long _lastTicks;

            public ProtoChannelTestClient(IStatistics statistics, TestClientSettings settings)
                : base(statistics, settings)
            {
                _stopwatch.Start();

                _service = new ClientService(settings.Host, Constants.ProtoChannelPort);
            }

            public override void Start()
            {
                _lastTicks = _stopwatch.ElapsedTicks;

                Statistics.AddConnectOverhead(_lastTicks);

                SendMessage();
            }

            private void SendMessage()
            {
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

                long currentTicks = _stopwatch.ElapsedTicks;

                Statistics.AddSendMessage(currentTicks - _lastTicks);

                _lastTicks = currentTicks;

                _messagesSend++;

                if (_messagesSend == Settings.RequestsPerClient)
                {
                    OnCompleted(EventArgs.Empty);

                    _service.Dispose();

                    Statistics.AddDisconnectOverhead(_stopwatch.ElapsedTicks - _lastTicks);
                }
                else
                {
                    SendMessage();
                }
            }
        }
    }
}
