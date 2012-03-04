using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoChannel.Demo
{
    internal abstract class TestClientRunner
    {
        public IStatistics Statistics { get; private set; }
        public TestClientSettings Settings { get; private set; }

        protected TestClientRunner(IStatistics statistics, TestClientSettings settings)
        {
            if (statistics == null)
                throw new ArgumentNullException("statistics");
            if (settings == null)
                throw new ArgumentNullException("settings");

            Statistics = statistics;
            Settings = settings;
        }

        public abstract TestClient CreateClient();
    }
}
