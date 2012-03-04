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
            this.label2.TabIndex = 1;
            this.label2.Text = "Clients completed:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 53);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(107, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Requests completed:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 72);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(128, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Average time per request:";
            // 
            // _clientsRunning
            // 
            this._clientsRunning.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._clientsRunning.Location = new System.Drawing.Point(149, 15);
            this._clientsRunning.Name = "_clientsRunning";
            this._clientsRunning.ReadOnly = true;
            this._clientsRunning.Size = new System.Drawing.Size(224, 13);
            this._clientsRunning.TabIndex = 4;
            // 
            // _clientsCompleted
            // 
            this._clientsCompleted.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._clientsCompleted.Location = new System.Drawing.Point(149, 34);
            this._clientsCompleted.Name = "_clientsCompleted";
            this._clientsCompleted.ReadOnly = true;
            this._clientsCompleted.Size = new System.Drawing.Size(224, 13);
            this._clientsCompleted.TabIndex = 4;
            // 
            // _requestsCompleted
            // 
            this._requestsCompleted.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._requestsCompleted.Location = new System.Drawing.Point(149, 53);
            this._requestsCompleted.Name = "_requestsCompleted";
            this._requestsCompleted.ReadOnly = true;
            this._requestsCompleted.Size = new System.Drawing.Size(224, 13);
            this._requestsCompleted.TabIndex = 4;
            // 
            // _timePerRequest
            // 
            this._timePerRequest.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._timePerRequest.Location = new System.Drawing.Point(149, 72);
            this._timePerRequest.Name = "_timePerRequest";
            this._timePerRequest.ReadOnly = true;
            this._timePerRequest.Size = new System.Drawing.Size(224, 13);
            this._timePerRequest.TabIndex = 4;
            // 
            // _refreshTimer
            // 
            this._refreshTimer.Enabled = true;
            this._refreshTimer.Tick += new System.EventHandler(this._refreshTimer_Tick);
            // 
            // TestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(388, 101);
            this.Controls.Add(this._timePerRequest);
            this.Controls.Add(this._requestsCompleted);
            this.Controls.Add(this._clientsCompleted);
            this.Controls.Add(this._clientsRunning);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
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
    }
}