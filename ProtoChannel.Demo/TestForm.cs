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
        private Queue<StreamTransferItem> _pendingTransfers = new Queue<StreamTransferItem>();
        private readonly Dictionary<TransferKey, ListViewItem> _transferItems = new Dictionary<TransferKey, ListViewItem>();

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

                client.StreamTransfer += client_StreamTransfer;
                client.Completed += client_Completed;

                client.Start();
            }
        }

        void client_StreamTransfer(object sender, StreamTransferEventArgs e)
        {
            lock (_syncRoot)
            {
                _pendingTransfers.Enqueue(new StreamTransferItem((TestClient)sender, e));
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

        private void _updateTransferring_Tick(object sender, EventArgs e)
        {
            _listView.BeginUpdate();

            Queue<StreamTransferItem> queue;

            lock (_syncRoot)
            {
                queue = _pendingTransfers;
                _pendingTransfers = new Queue<StreamTransferItem>();
            }

            foreach (var item in queue)
            {
                switch (item.EventArgs.EventType)
                {
                    case StreamTransferEventType.Start:
                        ProcessStartEvent(item);
                        break;

                    case StreamTransferEventType.Transfer:
                        ProcessTransferEvent(item);
                        break;

                    case StreamTransferEventType.End:
                        ProcessEndEvent(item);
                        break;
                }
            }

            _listView.EndUpdate();
        }

        private void ProcessStartEvent(StreamTransferItem item)
        {
            var key = new TransferKey(item.Client, item.EventArgs.StreamId);

            if (_transferItems.ContainsKey(key))
            {
                Debug.Fail("Did not expect the item to already exist");
                return;
            }

            var listViewItem = new ListViewItem
            {
                Text = item.EventArgs.StreamId.ToString(),
                SubItems =
                {
                    item.EventArgs.StreamName,
                    "",
                    ""
                }
            };

            UpdateListViewItem(listViewItem, item.EventArgs);

            _transferItems.Add(key, listViewItem);

            _listView.Items.Add(listViewItem);
        }

        private void UpdateListViewItem(ListViewItem listViewItem, StreamTransferEventArgs eventArgs)
        {
            listViewItem.SubItems[2].Text = eventArgs.Length.ToString();
            listViewItem.SubItems[3].Text = (eventArgs.Transferred * 100 / eventArgs.Length).ToString() + " %";
        }

        private void ProcessTransferEvent(StreamTransferItem item)
        {
            var key = new TransferKey(item.Client, item.EventArgs.StreamId);
            ListViewItem listViewItem;

            if (!_transferItems.TryGetValue(key, out listViewItem))
            {
                Debug.Fail("Expected a transfer");
                return;
            }

            UpdateListViewItem(listViewItem, item.EventArgs);
        }

        private void ProcessEndEvent(StreamTransferItem item)
        {
            var key = new TransferKey(item.Client, item.EventArgs.StreamId);
            ListViewItem listViewItem;

            if (!_transferItems.TryGetValue(key, out listViewItem))
            {
                Debug.Fail("Expected a transfer");
                return;
            }

            _transferItems.Remove(key);

            listViewItem.Remove();
        }

        private struct TransferKey
        {
            private readonly TestClient _client;
            private readonly int _id;

            public TransferKey(TestClient client, int id)
            {
                _client = client;
                _id = id;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is TransferKey))
                    return false;

                var other = (TransferKey)obj;

                return _client == other._client && _id == other._id;
            }

            public override int GetHashCode()
            {
                return _client.GetHashCode() ^ _id.GetHashCode();
            }
        }

        private class StreamTransferItem
        {
            public TestClient Client { get; private set; }
            public StreamTransferEventArgs EventArgs { get; private set; }

            public StreamTransferItem(TestClient client, StreamTransferEventArgs eventArgs)
            {
                Client = client;
                EventArgs = eventArgs;
            }
        }
    }

#if _NET_2
    public delegate void Action();
#endif
}
