using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ProtoChannel.Demo
{
    public partial class TestForm : Form, IStatistics
    {
        private readonly object _syncRoot = new object();
        private readonly TestClientRunner _clientRunner;
        private bool _completed;
        private Dictionary<TestClient, bool> _clients = new Dictionary<TestClient, bool>();
        private int _clientsRunningValue;
        private int _clientsCompletedValue;
        private readonly TickCounter _timePerRequestValue = new TickCounter();
        private readonly TickCounter _connectOverhead = new TickCounter();
        private readonly TickCounter _disconnectOverhead = new TickCounter();
        private AutoResetEvent _clientCompletedEvent = new AutoResetEvent(false);

        internal TestForm(TestClientSettings settings, TestMode mode)
        {
            if (mode == TestMode.ProtoChannel)
                _clientRunner = new ProtoChannelClientRunner(this, settings);
#if _NET_4
            else
                _clientRunner = new WcfClientRunner(this, settings);
#endif


            InitializeComponent();
        }

        private void TestForm_Shown(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(p => ClientConnector());
        }

        private void ClientConnector()
        {
            for (int i = 0; i < _clientRunner.Settings.TotalClients; i++)
            {
                while (_clientsRunningValue >= _clientRunner.Settings.ConcurrentClients)
                {
                    _clientCompletedEvent.WaitOne();
                }

                var client = _clientRunner.CreateClient();

                lock (_syncRoot)
                {
                    _clientsRunningValue++;

                    _clients.Add(client, true);
                }

                client.Completed += client_Completed;

                client.Start();
            }
        }

        private void Done()
        {
            _completed = true;

            MessageBox.Show(
                this,
                "The tests have completed",
                GetType().Assembly.GetName().Name,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        void client_Completed(object sender, EventArgs e)
        {
            lock (_syncRoot)
            {
                _clientsRunningValue--;
                _clientsCompletedValue++;

                _clientCompletedEvent.Set();

                if (_clientsCompletedValue >= _clientRunner.Settings.TotalClients)
                    BeginInvoke(new Action(Done));
            }
        }

        private void TestForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && !_completed)
            {
                MessageBox.Show(
                    this,
                    "The tests are still running and cannot be aborted.",
                    GetType().Assembly.GetName().Name,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                e.Cancel = true;
            }
        }

        private void _refreshTimer_Tick(object sender, EventArgs e)
        {
            _clientsRunning.Text = _clientsRunningValue.ToString();
            _clientsCompleted.Text = _clientsCompletedValue.ToString();
            _requestsCompleted.Text = _timePerRequestValue.Count.ToString();
            _timePerRequest.Text = _timePerRequestValue.Value.ToString("0.000 ms");
            _overhead.Text = (_connectOverhead.Value + _disconnectOverhead.Value).ToString("0.000 ms");
        }

        public void AddSendMessage(long ticks)
        {
            lock (_syncRoot)
            {
                _timePerRequestValue.Add(ticks);
            }
        }

        public void AddConnectOverhead(long ticks)
        {
            lock (_syncRoot)
            {
                _connectOverhead.Add(ticks);
            }
        }

        public void AddDisconnectOverhead(long ticks)
        {
            lock (_syncRoot)
            {
                _disconnectOverhead.Add(ticks);
            }
        }
    }

#if _NET_2
    public delegate void Action();
#endif
}
