using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ProtoChannel.Demo.ProtoService;
using ProtoChannel.Demo.Shared;

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
            private static readonly ComplexMessage _complexMessage;
            private static readonly byte[] _smallStreamContent;
            private static readonly byte[] _largeStreamContent;

            static ProtoChannelTestClient()
            {
                _complexMessage = new ComplexMessage();

                for (int i = 0; i < 200; i++)
                {
                    _complexMessage.Values.Add(new ComplexValue
                    {
                        IntValue = i,
                        DoubleValue = i,
                        StringValue = i.ToString()
                    });
                }

                _smallStreamContent = Encoding.UTF8.GetBytes(new string('x', 0x1000));
                _largeStreamContent = Encoding.UTF8.GetBytes(new string('x', 0x100000));
            }

            private readonly ClientService _service;
            private readonly ClientCallbackService _callbackService;
            private int _messagesSend;
            private readonly Stopwatch _stopwatch = new Stopwatch();
            private long _lastTicks;

            public ProtoChannelTestClient(IStatistics statistics, TestClientSettings settings)
                : base(statistics, settings)
            {
                _stopwatch.Start();

                _callbackService = new ClientCallbackService();

                _callbackService.StreamReceived += _callbackService_StreamReceived;

                var configuration = new ProtoClientConfiguration
                {
                    CallbackObject = _callbackService
                };

                _service = new ClientService(settings.Host, Constants.ProtoChannelPort, configuration);
            }

            void _callbackService_StreamReceived(object sender, EventArgs e)
            {
                ProcessMessageSend();
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

                    case ClientMessageType.Complex:
                        _service.BeginComplexMessage(
                            _complexMessage,
                            BeginComplexMessageCallback,
                            null
                        );
                        break;

                    case ClientMessageType.SmallStream:
                    case ClientMessageType.LargeStream:
                        var streamId = _service.SendStream(
                            new MemoryStream(
                                Settings.MessageType == ClientMessageType.SmallStream
                                ? _smallStreamContent
                                : _largeStreamContent
                            ),
                            "Small stream.dat",
                            "application/octet-stream"
                        );

                        _service.StreamMessage(new StreamMessage
                        {
                            StreamId = (uint)streamId
                        });
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }

            private void BeginSimpleMessageCallback(IAsyncResult asyncResult)
            {
                var result = _service.EndSimpleMessage(asyncResult);

                Debug.Assert(result.Value == _messagesSend);

                ProcessMessageSend();
            }

            private void BeginComplexMessageCallback(IAsyncResult asyncResult)
            {
                _service.EndComplexMessage(asyncResult);

                ProcessMessageSend();
            }

            private void ProcessMessageSend()
            {
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
