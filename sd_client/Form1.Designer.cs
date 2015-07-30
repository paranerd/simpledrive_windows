namespace sd_client
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.server_label = new System.Windows.Forms.Label();
            this.user_label = new System.Windows.Forms.Label();
            this.pass_label = new System.Windows.Forms.Label();
            this.folder_label = new System.Windows.Forms.Label();
            this.server_input = new System.Windows.Forms.TextBox();
            this.user_input = new System.Windows.Forms.TextBox();
            this.pass_input = new System.Windows.Forms.TextBox();
            this.folder_input = new System.Windows.Forms.Label();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.browse = new System.Windows.Forms.Button();
            this.connect = new System.Windows.Forms.Button();
            this.status = new System.Windows.Forms.Label();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.synchronizeNowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.simpleDriveInExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(13, 13);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(311, 144);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // server_label
            // 
            this.server_label.AutoSize = true;
            this.server_label.Location = new System.Drawing.Point(13, 166);
            this.server_label.Name = "server_label";
            this.server_label.Size = new System.Drawing.Size(59, 20);
            this.server_label.TabIndex = 1;
            this.server_label.Text = "Server:";
            // 
            // user_label
            // 
            this.user_label.AutoSize = true;
            this.user_label.Location = new System.Drawing.Point(13, 199);
            this.user_label.Name = "user_label";
            this.user_label.Size = new System.Drawing.Size(47, 20);
            this.user_label.TabIndex = 2;
            this.user_label.Text = "User:";
            // 
            // pass_label
            // 
            this.pass_label.AutoSize = true;
            this.pass_label.Location = new System.Drawing.Point(13, 238);
            this.pass_label.Name = "pass_label";
            this.pass_label.Size = new System.Drawing.Size(82, 20);
            this.pass_label.TabIndex = 3;
            this.pass_label.Text = "Password:";
            // 
            // folder_label
            // 
            this.folder_label.AutoSize = true;
            this.folder_label.Location = new System.Drawing.Point(13, 282);
            this.folder_label.Name = "folder_label";
            this.folder_label.Size = new System.Drawing.Size(58, 20);
            this.folder_label.TabIndex = 4;
            this.folder_label.Text = "Folder:";
            // 
            // server_input
            // 
            this.server_input.Location = new System.Drawing.Point(128, 163);
            this.server_input.Name = "server_input";
            this.server_input.Size = new System.Drawing.Size(196, 26);
            this.server_input.TabIndex = 5;
            // 
            // user_input
            // 
            this.user_input.Location = new System.Drawing.Point(128, 198);
            this.user_input.Name = "user_input";
            this.user_input.Size = new System.Drawing.Size(196, 26);
            this.user_input.TabIndex = 6;
            // 
            // pass_input
            // 
            this.pass_input.Location = new System.Drawing.Point(128, 235);
            this.pass_input.Name = "pass_input";
            this.pass_input.PasswordChar = '*';
            this.pass_input.Size = new System.Drawing.Size(196, 26);
            this.pass_input.TabIndex = 7;
            // 
            // folder_input
            // 
            this.folder_input.AutoSize = true;
            this.folder_input.Location = new System.Drawing.Point(124, 282);
            this.folder_input.Name = "folder_input";
            this.folder_input.Size = new System.Drawing.Size(0, 20);
            this.folder_input.TabIndex = 8;
            // 
            // browse
            // 
            this.browse.Location = new System.Drawing.Point(268, 277);
            this.browse.Name = "browse";
            this.browse.Size = new System.Drawing.Size(56, 31);
            this.browse.TabIndex = 9;
            this.browse.Text = "...";
            this.browse.UseVisualStyleBackColor = true;
            this.browse.Click += new System.EventHandler(this.browse_Click);
            // 
            // connect
            // 
            this.connect.Location = new System.Drawing.Point(180, 323);
            this.connect.Name = "connect";
            this.connect.Size = new System.Drawing.Size(144, 49);
            this.connect.TabIndex = 10;
            this.connect.Text = "Connect";
            this.connect.UseVisualStyleBackColor = true;
            this.connect.Click += new System.EventHandler(this.connect_Click);
            // 
            // status
            // 
            this.status.AutoSize = true;
            this.status.Location = new System.Drawing.Point(13, 372);
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(0, 20);
            this.status.TabIndex = 11;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.synchronizeNowToolStripMenuItem,
            this.simpleDriveInExplorerToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(266, 94);
            // 
            // synchronizeNowToolStripMenuItem
            // 
            this.synchronizeNowToolStripMenuItem.Name = "synchronizeNowToolStripMenuItem";
            this.synchronizeNowToolStripMenuItem.Size = new System.Drawing.Size(265, 30);
            this.synchronizeNowToolStripMenuItem.Text = "Synchronize Now";
            this.synchronizeNowToolStripMenuItem.Click += new System.EventHandler(this.synchronizeNowToolStripMenuItem_Click);
            // 
            // simpleDriveInExplorerToolStripMenuItem
            // 
            this.simpleDriveInExplorerToolStripMenuItem.Name = "simpleDriveInExplorerToolStripMenuItem";
            this.simpleDriveInExplorerToolStripMenuItem.Size = new System.Drawing.Size(265, 30);
            this.simpleDriveInExplorerToolStripMenuItem.Text = "simpleDrive in Explorer";
            this.simpleDriveInExplorerToolStripMenuItem.Click += new System.EventHandler(this.simpleDriveInExplorerToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(265, 30);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "notifyIcon1";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseClick);
            // 
            // Form1
            // 
            this.AcceptButton = this.connect;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(336, 410);
            this.Controls.Add(this.status);
            this.Controls.Add(this.connect);
            this.Controls.Add(this.browse);
            this.Controls.Add(this.folder_input);
            this.Controls.Add(this.pass_input);
            this.Controls.Add(this.user_input);
            this.Controls.Add(this.server_input);
            this.Controls.Add(this.folder_label);
            this.Controls.Add(this.pass_label);
            this.Controls.Add(this.user_label);
            this.Controls.Add(this.server_label);
            this.Controls.Add(this.pictureBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.Text = "simpleDrive";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label server_label;
        private System.Windows.Forms.Label user_label;
        private System.Windows.Forms.Label pass_label;
        private System.Windows.Forms.Label folder_label;
        private System.Windows.Forms.TextBox server_input;
        private System.Windows.Forms.TextBox user_input;
        private System.Windows.Forms.TextBox pass_input;
        private System.Windows.Forms.Label folder_input;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Button browse;
        private System.Windows.Forms.Button connect;
        private System.Windows.Forms.Label status;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem synchronizeNowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem simpleDriveInExplorerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
    }
}

