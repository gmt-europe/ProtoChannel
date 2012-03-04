using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Security;
using System.ServiceModel;
using System.Text;
using ProtoChannel.Demo.ServiceReference;

namespace ProtoChannel.Demo
{
    internal class WcfClientRunner : TestClientRunner
    {
        public WcfClientRunner(IStatistics statistics, TestClientSettings settings)
            : base(statistics, settings)
        {
        }

        public override TestClient CreateClient()
        {
            return new WcfTestClient(Statistics, Settings);
        }

        private class WcfTestClient : TestClient
        {
            private static readonly ComplexMessage _complexMessage;

            static WcfTestClient()
            {
                _complexMessage = new ComplexMessage();

                var complexValues = new List<ComplexValue>();

                for (int i = 0; i < 200; i++)
                {
                    complexValues.Add(new ComplexValue
                    {
                        IntValue = i,
                        DoubleValue = i,
                        StringValue = i.ToString()
                    });
                }

                _complexMessage.Values = complexValues.ToArray();
            }

            private readonly ServerServiceClient _service;
            private int _messagesSend;
            private readonly Stopwatch _stopwatch = new Stopwatch();
            private long _lastTicks;

            public WcfTestClient(IStatistics statistics, TestClientSettings settings)
                : base(statistics, settings)
            {
                _stopwatch.Start();

                _service = new ServerServiceClient(
                    new NetTcpBinding
                    {
                        Security = new NetTcpSecurity
                        {
                            Mode = SecurityMode.None,
                            Transport = new TcpTransportSecurity
                            {
                                ProtectionLevel = ProtectionLevel.None
                            },
                            Message = new MessageSecurityOverTcp
                            {
                                ClientCredentialType = MessageCredentialType.None
                            }
                        }
                    },
                    new EndpointAddress("net.tcp://" + Settings.Host + ":" + Constants.WcfPort + "/ServerService/")
                );

                _service.Open();
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

                    _service.Close();

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
