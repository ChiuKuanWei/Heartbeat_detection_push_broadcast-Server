namespace LEON_Can4G
{
    partial class frmMain
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pnlLastThree = new System.Windows.Forms.Panel();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnListen = new System.Windows.Forms.Button();
            this.lblPort = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.lblIPAddress = new System.Windows.Forms.Label();
            this.txtIPAddress = new System.Windows.Forms.TextBox();
            this.tmrShow = new System.Windows.Forms.Timer(this.components);
            this.tmrUpdate = new System.Windows.Forms.Timer(this.components);
            this.btnAlive = new System.Windows.Forms.Button();
            this.lblMsg = new System.Windows.Forms.Label();
            this.rtbMsg = new System.Windows.Forms.RichTextBox();
            this.pnlLastThree.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlLastThree
            // 
            this.pnlLastThree.Controls.Add(this.richTextBox1);
            this.pnlLastThree.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlLastThree.Location = new System.Drawing.Point(0, 104);
            this.pnlLastThree.Name = "pnlLastThree";
            this.pnlLastThree.Size = new System.Drawing.Size(741, 513);
            this.pnlLastThree.TabIndex = 1;
            // 
            // richTextBox1
            // 
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.Location = new System.Drawing.Point(0, 0);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(741, 513);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.rtbMsg);
            this.panel1.Controls.Add(this.lblMsg);
            this.panel1.Controls.Add(this.btnAlive);
            this.panel1.Controls.Add(this.btnListen);
            this.panel1.Controls.Add(this.lblPort);
            this.panel1.Controls.Add(this.txtPort);
            this.panel1.Controls.Add(this.lblIPAddress);
            this.panel1.Controls.Add(this.txtIPAddress);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(741, 101);
            this.panel1.TabIndex = 2;
            // 
            // btnListen
            // 
            this.btnListen.Location = new System.Drawing.Point(401, 11);
            this.btnListen.Name = "btnListen";
            this.btnListen.Size = new System.Drawing.Size(75, 25);
            this.btnListen.TabIndex = 4;
            this.btnListen.Text = "監聽";
            this.btnListen.UseVisualStyleBackColor = true;
            this.btnListen.Click += new System.EventHandler(this.btnListen_Click);
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(254, 15);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(34, 17);
            this.lblPort.TabIndex = 3;
            this.lblPort.Text = "Port";
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(294, 12);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(71, 25);
            this.txtPort.TabIndex = 2;
            this.txtPort.Text = "3657";
            // 
            // lblIPAddress
            // 
            this.lblIPAddress.AutoSize = true;
            this.lblIPAddress.Location = new System.Drawing.Point(16, 15);
            this.lblIPAddress.Name = "lblIPAddress";
            this.lblIPAddress.Size = new System.Drawing.Size(69, 17);
            this.lblIPAddress.TabIndex = 1;
            this.lblIPAddress.Text = "IPAddress";
            // 
            // txtIPAddress
            // 
            this.txtIPAddress.Location = new System.Drawing.Point(91, 12);
            this.txtIPAddress.Name = "txtIPAddress";
            this.txtIPAddress.Size = new System.Drawing.Size(135, 25);
            this.txtIPAddress.TabIndex = 0;
            this.txtIPAddress.Text = "61.216.46.170";
            // 
            // tmrShow
            // 
            this.tmrShow.Interval = 15000;
            this.tmrShow.Tick += new System.EventHandler(this.tmrShow_Tick);
            // 
            // tmrUpdate
            // 
            this.tmrUpdate.Interval = 1000;
            this.tmrUpdate.Tick += new System.EventHandler(this.tmrUpdate_Tick);
            // 
            // btnAlive
            // 
            this.btnAlive.Location = new System.Drawing.Point(621, 11);
            this.btnAlive.Name = "btnAlive";
            this.btnAlive.Size = new System.Drawing.Size(108, 25);
            this.btnAlive.TabIndex = 5;
            this.btnAlive.Text = "心跳啟動";
            this.btnAlive.UseVisualStyleBackColor = true;
            this.btnAlive.Click += new System.EventHandler(this.btnAlive_Click);
            // 
            // lblMsg
            // 
            this.lblMsg.AutoSize = true;
            this.lblMsg.Location = new System.Drawing.Point(12, 45);
            this.lblMsg.Name = "lblMsg";
            this.lblMsg.Size = new System.Drawing.Size(60, 17);
            this.lblMsg.TabIndex = 6;
            this.lblMsg.Text = "系統訊息";
            // 
            // rtbMsg
            // 
            this.rtbMsg.Location = new System.Drawing.Point(91, 45);
            this.rtbMsg.Name = "rtbMsg";
            this.rtbMsg.Size = new System.Drawing.Size(638, 53);
            this.rtbMsg.TabIndex = 7;
            this.rtbMsg.Text = "";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(741, 617);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.pnlLastThree);
            this.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "frmMain";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.pnlLastThree.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel pnlLastThree;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnListen;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Label lblIPAddress;
        private System.Windows.Forms.TextBox txtIPAddress;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Timer tmrShow;
        private System.Windows.Forms.Timer tmrUpdate;
        private System.Windows.Forms.Button btnAlive;
        private System.Windows.Forms.Label lblMsg;
        private System.Windows.Forms.RichTextBox rtbMsg;
    }
}

