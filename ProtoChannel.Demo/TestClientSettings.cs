using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoChannel.Demo
{
    internal class TestClientSettings
    {
        public string Host { get; private set; }
        public int ConcurrentClients { get; private set; }
        public int TotalClients { get; private set; }
        public int RequestsPerClient { get; private set; }
        public ClientMessageType MessageType { get; private set; }

        public TestClientSettings(string host, int concurrentClients, int totalClients, int requestsPerClient, ClientMessageType messageType)
        {
            Host = host;
            ConcurrentClients = concurrentClients;
            TotalClients = totalClients;
            RequestsPerClient = requestsPerClient;
            MessageType = messageType;
        }
    }

    internal enum ClientMessageType
    {
        Simple,
        Complex,
        SmallStream,
        LargeStream
    }
}
