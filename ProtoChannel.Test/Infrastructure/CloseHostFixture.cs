using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using NUnit.Framework;
using ProtoChannel.Test.Service;

namespace ProtoChannel.Test.Infrastructure
{
    [TestFixture]
    internal class CloseHostFixture : FixtureBase
    {
        [Test]
        public void CloseGracefully()
        {
            using (var host = new ProtoHost<ServerService>(new IPEndPoint(IPAddress.Loopback, 0)))
            {
                using (new ClientService(host.LocalEndPoint))
                {
                    var stopwatch = new Stopwatch();

                    stopwatch.Start();
                    host.Close(CloseMode.Gracefully, TimeSpan.FromMilliseconds(100));
                    stopwatch.Stop();

                    Assert.GreaterOrEqual(stopwatch.ElapsedMilliseconds, 100);
                }
            }
        }

        [Test]
        public void CloseWithAbort()
        {
            using (var host = new ProtoHost<ServerService>(new IPEndPoint(IPAddress.Loopback, 0)))
            {
                using (new ClientService(host.LocalEndPoint))
                {
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    host.Close(CloseMode.Abort);

                    stopwatch.Stop();

                    // Asserted time is not exact, but it will probably be less
                    // than this.

                    Assert.Less(stopwatch.ElapsedMilliseconds, 100);
                }
            }
        }

        [Test]
        public void GracefullCloseWithoutFullTimeout()
        {
            using (var host = new ProtoHost<ServerService>(new IPEndPoint(IPAddress.Loopback, 0)))
            {
                var stopwatch = new Stopwatch();
                
                var client = new ClientService(host.LocalEndPoint);

                ThreadPool.QueueUserWorkItem(p =>
                {
                    using (client)
                    {
                        Thread.Sleep(100);
                    }
                });

                stopwatch.Start();
                host.Close(CloseMode.Gracefully, TimeSpan.FromMilliseconds(500));
                stopwatch.Stop();

                Assert.GreaterOrEqual(stopwatch.ElapsedMilliseconds, 100);
                Assert.LessOrEqual(stopwatch.ElapsedMilliseconds, 150);
            }
        }
    }
}
