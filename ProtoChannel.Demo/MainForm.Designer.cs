namespace ProtoChannel.Demo
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this._protoChannelPort = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this._wcfPort = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._requestPerClient = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this._totalClients = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this._messageSimple = new System.Windows.Forms.RadioButton();
            this._messageComplex = new System.Windows.Forms.RadioButton();
            this._messageSmallStream = new System.Windows.Forms.RadioButton();
            this._messageLargeStream = new System.Windows.Forms.RadioButton();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this._modeProtoChannel = new System.Windows.Forms.RadioButton();
            this._modeWcf = new System.Windows.Forms.RadioButton();
            this._concurrentClients = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this._acceptButton = new System.Windows.Forms.Button();
            this._errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.label6 = new System.Windows.Forms.Label();
            this._host = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(127, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "ProtoChannel server port:";
            // 
            // _protoChannelPort
            // 
            this._protoChannelPort.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._protoChannelPort.Location = new System.Drawing.Point(148, 15);
            this._protoChannelPort.Name = "_protoChannelPort";
            this._protoChannelPort.ReadOnly = true;
            this._protoChannelPort.Size = new System.Drawing.Size(238, 13);
            this._protoChannelPort.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 34);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "WCF server port:";
            // 
            // _wcfPort
            // 
            this._wcfPort.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._wcfPort.Location = new System.Drawing.Point(148, 34);
            this._wcfPort.Name = "_wcfPort";
            this._wcfPort.ReadOnly = true;
            this._wcfPort.Size = new System.Drawing.Size(238, 13);
            this._wcfPort.TabIndex = 3;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this._host);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this._requestPerClient);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this._totalClients);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.flowLayoutPanel2);
            this.groupBox1.Controls.Add(this.flowLayoutPanel1);
            this.groupBox1.Controls.Add(this._concurrentClients);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(15, 53);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(8, 4, 8, 8);
            this.groupBox1.Size = new System.Drawing.Size(371, 281);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Client settings";
            // 
            // _requestPerClient
            // 
            this._requestPerClient.Location = new System.Drawing.Point(113, 151);
            this._requestPerClient.Name = "_requestPerClient";
            this._requestPerClient.Size = new System.Drawing.Size(247, 20);
            this._requestPerClient.TabIndex = 8;
            this._requestPerClient.Text = "10";
            this._requestPerClient.Validating += new System.ComponentModel.CancelEventHandler(this._requestPerClient_Validating);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(11, 154);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(96, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "Request per client:";
            // 
            // _totalClients
            // 
            this._totalClients.Location = new System.Drawing.Point(113, 125);
            this._totalClients.Name = "_totalClients";
            this._totalClients.Size = new System.Drawing.Size(247, 20);
            this._totalClients.TabIndex = 6;
            this._totalClients.Text = "10";
            this._totalClients.Validating += new System.ComponentModel.CancelEventHandler(this._totalClients_Validating);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 128);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(67, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Total clients:";
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.AutoSize = true;
            this.flowLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel2.Controls.Add(this._messageSimple);
            this.flowLayoutPanel2.Controls.Add(this._messageComplex);
            this.flowLayoutPanel2.Controls.Add(this._messageSmallStream);
            this.flowLayoutPanel2.Controls.Add(this._messageLargeStream);
            this.flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(11, 177);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(121, 92);
            this.flowLayoutPanel2.TabIndex = 9;
            // 
            // _messageSimple
            // 
            this._messageSimple.AutoSize = true;
            this._messageSimple.Checked = true;
            this._messageSimple.Location = new System.Drawing.Point(3, 3);
            this._messageSimple.Name = "_messageSimple";
            this._messageSimple.Size = new System.Drawing.Size(106, 17);
            this._messageSimple.TabIndex = 0;
            this._messageSimple.TabStop = true;
            this._messageSimple.Text = "Simple messages";
            this._messageSimple.UseVisualStyleBackColor = true;
            // 
            // _messageComplex
            // 
            this._messageComplex.AutoSize = true;
            this._messageComplex.Location = new System.Drawing.Point(3, 26);
            this._messageComplex.Name = "_messageComplex";
            this._messageComplex.Size = new System.Drawing.Size(115, 17);
            this._messageComplex.TabIndex = 1;
            this._messageComplex.Text = "Complex messages";
            this._messageComplex.UseVisualStyleBackColor = true;
            // 
            // _messageSmallStream
            // 
            this._messageSmallStream.AutoSize = true;
            this._messageSmallStream.Location = new System.Drawing.Point(3, 49);
            this._messageSmallStream.Name = "_messageSmallStream";
            this._messageSmallStream.Size = new System.Drawing.Size(89, 17);
            this._messageSmallStream.TabIndex = 2;
            this._messageSmallStream.Text = "Small streams";
            this._messageSmallStream.UseVisualStyleBackColor = true;
            // 
            // _messageLargeStream
            // 
            this._messageLargeStream.AutoSize = true;
            this._messageLargeStream.Location = new System.Drawing.Point(3, 72);
            this._messageLargeStream.Name = "_messageLargeStream";
            this._messageLargeStream.Size = new System.Drawing.Size(91, 17);
            this._messageLargeStream.TabIndex = 3;
            this._messageLargeStream.Text = "Large streams";
            this._messageLargeStream.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this._modeProtoChannel);
            this.flowLayoutPanel1.Controls.Add(this._modeWcf);
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(12, 21);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(95, 46);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // _modeProtoChannel
            // 
            this._modeProtoChannel.AutoSize = true;
            this._modeProtoChannel.Checked = true;
            this._modeProtoChannel.Location = new System.Drawing.Point(3, 3);
            this._modeProtoChannel.Name = "_modeProtoChannel";
            this._modeProtoChannel.Size = new System.Drawing.Size(89, 17);
            this._modeProtoChannel.TabIndex = 0;
            this._modeProtoChannel.TabStop = true;
            this._modeProtoChannel.Text = "ProtoChannel";
            this._modeProtoChannel.UseVisualStyleBackColor = true;
            // 
            // _modeWcf
            // 
            this._modeWcf.AutoSize = true;
            this._modeWcf.Location = new System.Drawing.Point(3, 26);
            this._modeWcf.Name = "_modeWcf";
            this._modeWcf.Size = new System.Drawing.Size(49, 17);
            this._modeWcf.TabIndex = 1;
            this._modeWcf.Text = "WCF";
            this._modeWcf.UseVisualStyleBackColor = true;
            // 
            // _concurrentClients
            // 
            this._concurrentClients.Location = new System.Drawing.Point(113, 99);
            this._concurrentClients.Name = "_concurrentClients";
            this._concurrentClients.Size = new System.Drawing.Size(247, 20);
            this._concurrentClients.TabIndex = 4;
            this._concurrentClients.Text = "10";
            this._concurrentClients.Validating += new System.ComponentModel.CancelEventHandler(this._concurrentClients_Validating);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 102);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(95, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Concurrent clients:";
            // 
            // _acceptButton
            // 
            this._acceptButton.Location = new System.Drawing.Point(296, 340);
            this._acceptButton.Name = "_acceptButton";
            this._acceptButton.Size = new System.Drawing.Size(90, 23);
            this._acceptButton.TabIndex = 5;
            this._acceptButton.Text = "Start Client";
            this._acceptButton.UseVisualStyleBackColor = true;
            this._acceptButton.Click += new System.EventHandler(this._acceptButton_Click);
            // 
            // _errorProvider
            // 
            this._errorProvider.ContainerControl = this;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(11, 76);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(32, 13);
            this.label6.TabIndex = 1;
            this.label6.Text = "Host:";
            // 
            // _host
            // 
            this._host.Location = new System.Drawing.Point(113, 73);
            this._host.Name = "_host";
            this._host.Size = new System.Drawing.Size(247, 20);
            this._host.TabIndex = 2;
            this._host.Text = "localhost";
            // 
            // MainForm
            // 
            this.AcceptButton = this._acceptButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(401, 378);
            this.Controls.Add(this._acceptButton);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this._wcfPort);
            this.Controls.Add(this.label2);
            this.Controls.Add(this._protoChannelPort);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.Padding = new System.Windows.Forms.Padding(12);
            this.Text = "ProtoChannel Client/Server Demo";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox _protoChannelPort;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox _wcfPort;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.RadioButton _messageSimple;
        private System.Windows.Forms.RadioButton _messageComplex;
        private System.Windows.Forms.RadioButton _messageSmallStream;
        private System.Windows.Forms.RadioButton _messageLargeStream;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.RadioButton _modeProtoChannel;
        private System.Windows.Forms.RadioButton _modeWcf;
        private System.Windows.Forms.TextBox _concurrentClients;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button _acceptButton;
        private System.Windows.Forms.ErrorProvider _errorProvider;
        private System.Windows.Forms.TextBox _totalClients;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox _requestPerClient;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox _host;
        private System.Windows.Forms.Label label6;
    }
}

