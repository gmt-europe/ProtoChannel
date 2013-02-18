﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoChannel.Demo
{
    internal abstract class TestClient
    {
        public IStatistics Statistics { get; private set; }
        public TestClientSettings Settings { get; private set; }

        public event EventHandler Completed;

        protected virtual void OnCompleted(EventArgs e)
        {
            var ev = Completed;

            if (ev != null)
                ev(this, e);
        }

        public event StreamTransferEventHandler StreamTransfer;

        protected virtual void OnStreamTransfer(StreamTransferEventArgs e)
        {
            var ev = StreamTransfer;
            if (ev != null)
                ev(this, e);
        }

        public void RaiseStreamTransfer(StreamTransferEventArgs e)
        {
            OnStreamTransfer(e);
        }

        protected TestClient(IStatistics statistics, TestClientSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            Statistics = statistics;
            Settings = settings;
        }

        public abstract void Start();
    }
}
