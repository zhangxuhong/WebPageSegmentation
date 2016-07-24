namespace bid
{
    partial class brownser
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.textBox_URL = new System.Windows.Forms.TextBox();
            this.webBrowser = new System.Windows.Forms.WebBrowser();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.valid = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CompleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.valid1 = new System.Windows.Forms.Button();
            this.Notice = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.bidTittleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bidNumberToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dueDayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bidDescriptionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nEXTToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.contextMenuStrip2.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox_URL
            // 
            this.textBox_URL.Location = new System.Drawing.Point(1, 2);
            this.textBox_URL.Name = "textBox_URL";
            this.textBox_URL.Size = new System.Drawing.Size(962, 20);
            this.textBox_URL.TabIndex = 0;
            this.textBox_URL.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_URL_KeyDown);
            // 
            // webBrowser
            // 
            this.webBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowser.Location = new System.Drawing.Point(1, 75);
            this.webBrowser.MinimumSize = new System.Drawing.Size(20, 22);
            this.webBrowser.Name = "webBrowser";
            this.webBrowser.Size = new System.Drawing.Size(962, 550);
            this.webBrowser.TabIndex = 2;
            this.webBrowser.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webBrowser_DocumentCompleted);
            // 
            // treeView1
            // 
            this.treeView1.AccessibleRole = System.Windows.Forms.AccessibleRole.ScrollBar;
            this.treeView1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.treeView1.Location = new System.Drawing.Point(792, 319);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(162, 306);
            this.treeView1.TabIndex = 4;
            this.treeView1.Visible = false;
            this.treeView1.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView1_NodeMouseClick);
            // 
            // valid
            // 
            this.valid.Location = new System.Drawing.Point(588, 24);
            this.valid.Name = "valid";
            this.valid.Size = new System.Drawing.Size(87, 45);
            this.valid.TabIndex = 5;
            this.valid.Text = "YES";
            this.valid.UseVisualStyleBackColor = true;
            this.valid.Click += new System.EventHandler(this.Yes_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 2000;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToolStripMenuItem,
            this.CompleteToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(127, 48);
            // 
            // addToolStripMenuItem
            // 
            this.addToolStripMenuItem.Name = "addToolStripMenuItem";
            this.addToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.addToolStripMenuItem.Text = "Add";
            this.addToolStripMenuItem.Click += new System.EventHandler(this.addToolStripMenuItem_Click);
            // 
            // CompleteToolStripMenuItem
            // 
            this.CompleteToolStripMenuItem.Name = "CompleteToolStripMenuItem";
            this.CompleteToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.CompleteToolStripMenuItem.Text = "Complete";
            this.CompleteToolStripMenuItem.Click += new System.EventHandler(this.CompleteToolStripMenuItem_Click);
            // 
            // valid1
            // 
            this.valid1.Location = new System.Drawing.Point(740, 24);
            this.valid1.Name = "valid1";
            this.valid1.Size = new System.Drawing.Size(87, 45);
            this.valid1.TabIndex = 6;
            this.valid1.Text = "NO";
            this.valid1.UseVisualStyleBackColor = true;
            this.valid1.Click += new System.EventHandler(this.valid1_Click);
            // 
            // Notice
            // 
            this.Notice.BackColor = System.Drawing.Color.Gold;
            this.Notice.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Notice.ForeColor = System.Drawing.Color.Red;
            this.Notice.Location = new System.Drawing.Point(116, 24);
            this.Notice.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.Notice.Name = "Notice";
            this.Notice.Size = new System.Drawing.Size(392, 45);
            this.Notice.TabIndex = 7;
            this.Notice.Text = "Plesae enter URL or Click Next";
            this.Notice.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(870, 28);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 40);
            this.button1.TabIndex = 8;
            this.button1.Text = "All in Current URL";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // contextMenuStrip2
            // 
            this.contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bidTittleToolStripMenuItem,
            this.bidNumberToolStripMenuItem,
            this.dueDayToolStripMenuItem,
            this.bidDescriptionToolStripMenuItem,
            this.nEXTToolStripMenuItem});
            this.contextMenuStrip2.Name = "contextMenuStrip2";
            this.contextMenuStrip2.Size = new System.Drawing.Size(154, 114);
            // 
            // bidTittleToolStripMenuItem
            // 
            this.bidTittleToolStripMenuItem.Name = "bidTittleToolStripMenuItem";
            this.bidTittleToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.bidTittleToolStripMenuItem.Text = "bid tittle";
            this.bidTittleToolStripMenuItem.Click += new System.EventHandler(this.bidTittleToolStripMenuItem_Click);
            // 
            // bidNumberToolStripMenuItem
            // 
            this.bidNumberToolStripMenuItem.Name = "bidNumberToolStripMenuItem";
            this.bidNumberToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.bidNumberToolStripMenuItem.Text = "bid number";
            this.bidNumberToolStripMenuItem.Click += new System.EventHandler(this.bidTittleToolStripMenuItem_Click);
            // 
            // dueDayToolStripMenuItem
            // 
            this.dueDayToolStripMenuItem.Name = "dueDayToolStripMenuItem";
            this.dueDayToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.dueDayToolStripMenuItem.Text = "due day";
            this.dueDayToolStripMenuItem.Click += new System.EventHandler(this.bidTittleToolStripMenuItem_Click);
            // 
            // bidDescriptionToolStripMenuItem
            // 
            this.bidDescriptionToolStripMenuItem.Name = "bidDescriptionToolStripMenuItem";
            this.bidDescriptionToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.bidDescriptionToolStripMenuItem.Text = "bid description";
            this.bidDescriptionToolStripMenuItem.Click += new System.EventHandler(this.bidTittleToolStripMenuItem_Click);
            // 
            // nEXTToolStripMenuItem
            // 
            this.nEXTToolStripMenuItem.Name = "nEXTToolStripMenuItem";
            this.nEXTToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.nEXTToolStripMenuItem.Text = "NEXT";
            this.nEXTToolStripMenuItem.Click += new System.EventHandler(this.nEXTToolStripMenuItem_Click);
            // 
            // brownser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(963, 635);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.Notice);
            this.Controls.Add(this.webBrowser);
            this.Controls.Add(this.valid1);
            this.Controls.Add(this.textBox_URL);
            this.Controls.Add(this.valid);
            this.Controls.Add(this.treeView1);
            this.MinimizeBox = false;
            this.Name = "brownser";
            this.Text = "Browser";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.contextMenuStrip1.ResumeLayout(false);
            this.contextMenuStrip2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_URL;
        private System.Windows.Forms.WebBrowser webBrowser;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.Button valid;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem CompleteToolStripMenuItem;
        private System.Windows.Forms.Button valid1;
        private System.Windows.Forms.Label Notice;
        private System.Windows.Forms.ToolStripMenuItem addToolStripMenuItem;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.ToolStripMenuItem bidTittleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bidNumberToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dueDayToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bidDescriptionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nEXTToolStripMenuItem;
    }
}

