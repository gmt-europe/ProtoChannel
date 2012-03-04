using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            throw new NotImplementedException();
        }
    }
}
