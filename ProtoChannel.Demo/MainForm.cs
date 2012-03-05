using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Net.Security;
#if _NET_4
using System.ServiceModel;
using System.ServiceModel.Description;
#endif
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Net;

namespace ProtoChannel.Demo
{
    public partial class MainForm : Form
    {
        private readonly ProtoHost<ProtoService.ServerService> _protoServer;
#if _NET_4
        private ServiceHost _wcfServer;
#endif

        public MainForm()
        {
            InitializeComponent();

            _protoServer = new ProtoHost<ProtoService.ServerService>(new IPEndPoint(IPAddress.Any, Constants.ProtoChannelPort));

#if _NET_4
            var waitEvent = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem(p =>
            {
                _wcfServer = new ServiceHost(typeof(Wcf.ServerService));

                _wcfServer.Open();

                waitEvent.Set();
            });

            waitEvent.WaitOne();
#else
            _modeWcf.Enabled = false;
#endif
        }

        private void _acceptButton_Click(object sender, EventArgs e)
        {
            if (ValidateChildren())
            {
                ClientMessageType messageType;

                if (_messageSimple.Checked)
                    messageType = ClientMessageType.Simple;
                else if (_messageComplex.Checked)
                    messageType = ClientMessageType.Complex;
                else if (_messageSmallStream.Checked)
                    messageType = ClientMessageType.SmallStream;
                else
                    messageType = ClientMessageType.LargeStream;

                var settings = new TestClientSettings(
                    _host.Text,
                    int.Parse(_concurrentClients.Text),
                    int.Parse(_totalClients.Text),
                    int.Parse(_requestPerClient.Text),
                    messageType
                    );

                using (var form = new TestForm(settings, _modeProtoChannel.Checked ? TestMode.ProtoChannel : TestMode.Wcf))
                {
                    form.ShowDialog(this);
                }
            }
        }

        private void _concurrentClients_Validating(object sender, CancelEventArgs e)
        {
            int result;
            string error = null;

            if (!int.TryParse(_concurrentClients.Text, out result) || result < 0)
                error = "Concurrent clients must be an integer greater than zero";

            _errorProvider.SetError(_concurrentClients, error);
        }

        private void _totalClients_Validating(object sender, CancelEventArgs e)
        {
            int result;
            string error = null;

            if (!int.TryParse(_totalClients.Text, out result) || result < 0)
                error = "Total clients must be an integer greater than zero";

            _errorProvider.SetError(_totalClients, error);
        }

        private void _requestPerClient_Validating(object sender, CancelEventArgs e)
        {
            int result;
            string error = null;

            if (!int.TryParse(_requestPerClient.Text, out result) || result < 0)
                error = "Requests per client must be an integer greater than zero";

            _errorProvider.SetError(_requestPerClient, error);
        }

        private void _modeProtoChannel_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        private void _modeWcf_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        private void UpdateEnabled()
        {
            _messageSmallStream.Enabled = _modeProtoChannel.Checked;
            _messageLargeStream.Enabled = _modeProtoChannel.Checked;

            if (_modeWcf.Checked && (_messageSmallStream.Checked || _messageLargeStream.Checked))
                _messageSimple.Checked = true;
        }
    }
}
