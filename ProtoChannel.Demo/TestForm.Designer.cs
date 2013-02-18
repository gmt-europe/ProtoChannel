namespace ProtoChannel.Demo
{
    partial class TestForm
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
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this._clientsRunning = new System.Windows.Forms.TextBox();
            this._clientsCompleted = new System.Windows.Forms.TextBox();
            this._requestsCompleted = new System.Windows.Forms.TextBox();
            this._timePerRequest = new System.Windows.Forms.TextBox();
            this._refreshTimer = new System.Windows.Forms.Timer(this.components);
            this._overhead = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this._listView = new ProtoChannel.Demo.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._updateTransferring = new System.Windows.Forms.Timer(this.components);
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Clients running:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 34);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(93, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Clients completed:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 53);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(107, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Requests completed:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 72);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(128, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Average time per request:";
            // 
            // _clientsRunning
            // 
            this._clientsRunning.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._clientsRunning.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._clientsRunning.Location = new System.Drawing.Point(149, 15);
            this._clientsRunning.Name = "_clientsRunning";
            this._clientsRunning.ReadOnly = true;
            this._clientsRunning.Size = new System.Drawing.Size(321, 13);
            this._clientsRunning.TabIndex = 1;
            // 
            // _clientsCompleted
            // 
            this._clientsCompleted.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._clientsCompleted.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._clientsCompleted.Location = new System.Drawing.Point(149, 34);
            this._clientsCompleted.Name = "_clientsCompleted";
            this._clientsCompleted.ReadOnly = true;
            this._clientsCompleted.Size = new System.Drawing.Size(321, 13);
            this._clientsCompleted.TabIndex = 3;
            // 
            // _requestsCompleted
            // 
            this._requestsCompleted.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._requestsCompleted.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._requestsCompleted.Location = new System.Drawing.Point(149, 53);
            this._requestsCompleted.Name = "_requestsCompleted";
            this._requestsCompleted.ReadOnly = true;
            this._requestsCompleted.Size = new System.Drawing.Size(321, 13);
            this._requestsCompleted.TabIndex = 5;
            // 
            // _timePerRequest
            // 
            this._timePerRequest.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._timePerRequest.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._timePerRequest.Location = new System.Drawing.Point(149, 72);
            this._timePerRequest.Name = "_timePerRequest";
            this._timePerRequest.ReadOnly = true;
            this._timePerRequest.Size = new System.Drawing.Size(321, 13);
            this._timePerRequest.TabIndex = 7;
            // 
            // _refreshTimer
            // 
            this._refreshTimer.Enabled = true;
            this._refreshTimer.Tick += new System.EventHandler(this._refreshTimer_Tick);
            // 
            // _overhead
            // 
            this._overhead.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._overhead.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._overhead.Location = new System.Drawing.Point(149, 91);
            this._overhead.Name = "_overhead";
            this._overhead.ReadOnly = true;
            this._overhead.Size = new System.Drawing.Size(321, 13);
            this._overhead.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(15, 91);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(57, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Overhead:";
            // 
            // _listView
            // 
            this._listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this._listView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._listView.FullRowSelect = true;
            this._listView.Location = new System.Drawing.Point(8, 17);
            this._listView.MultiSelect = false;
            this._listView.Name = "_listView";
            this._listView.Size = new System.Drawing.Size(439, 293);
            this._listView.TabIndex = 10;
            this._listView.UseCompatibleStateImageBehavior = false;
            this._listView.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Id";
            this.columnHeader1.Width = 49;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Stream Name";
            this.columnHeader2.Width = 153;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Length";
            this.columnHeader3.Width = 97;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Progress";
            this.columnHeader4.Width = 103;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this._listView);
            this.groupBox1.Location = new System.Drawing.Point(15, 110);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(8, 4, 8, 8);
            this.groupBox1.Size = new System.Drawing.Size(455, 318);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Transferring streams";
            // 
            // _updateTransferring
            // 
            this._updateTransferring.Enabled = true;
            this._updateTransferring.Interval = 1000;
            this._updateTransferring.Tick += new System.EventHandler(this._updateTransferring_Tick);
            // 
            // TestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(485, 443);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this._overhead);
            this.Controls.Add(this.label5);
            this.Controls.Add(this._timePerRequest);
            this.Controls.Add(this._requestsCompleted);
            this.Controls.Add(this._clientsCompleted);
            this.Controls.Add(this._clientsRunning);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TestForm";
            this.Padding = new System.Windows.Forms.Padding(12);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Running Tests";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TestForm_FormClosing);
            this.Shown += new System.EventHandler(this.TestForm_Shown);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox _clientsRunning;
        private System.Windows.Forms.TextBox _clientsCompleted;
        private System.Windows.Forms.TextBox _requestsCompleted;
        private System.Windows.Forms.TextBox _timePerRequest;
        private System.Windows.Forms.Timer _refreshTimer;
        private System.Windows.Forms.TextBox _overhead;
        private System.Windows.Forms.Label label5;
        private ProtoChannel.Demo.ListView _listView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Timer _updateTransferring;
    }
}