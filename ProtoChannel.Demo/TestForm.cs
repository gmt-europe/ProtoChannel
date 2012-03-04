using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
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
        private HashSet<TestClient> _clients = new HashSet<TestClient>();
        private int _clientsRunningValue;
        private int _clientsCompletedValue;
        private int _requestsCompletedValue;
        private double _timePerRequestValue;
        private long _totalTicks;
        private AutoResetEvent _clientCompletedEvent = new AutoResetEvent(false);

        internal TestForm(TestClientSettings settings, TestMode mode)
        {
            if (mode == TestMode.ProtoChannel)
                _clientRunner = new ProtoChannelClientRunner(this, settings);
            else
                _clientRunner = new WcfClientRunner(this, settings);


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

                    _clients.Add(client);
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
            _requestsCompleted.Text = _requestsCompletedValue.ToString();
            _timePerRequest.Text = _timePerRequestValue.ToString("0.000 ms");
        }

        public void AddSendMessage(long ticks)
        {
            lock (_syncRoot)
            {
                _totalTicks += ticks;
                _requestsCompletedValue++;
                _timePerRequestValue = ((double)(_totalTicks / _requestsCompletedValue) / Stopwatch.Frequency) * 1000d;
            }
        }
    }
}
