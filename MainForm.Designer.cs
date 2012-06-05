namespace GratisInc.Tools.FogBugz.WorkingOn
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
           System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
           this.tbServer = new System.Windows.Forms.TextBox();
           this.tbUser = new System.Windows.Forms.TextBox();
           this.tbPassword = new System.Windows.Forms.TextBox();
           this.lblServer = new System.Windows.Forms.Label();
           this.lblUser = new System.Windows.Forms.Label();
           this.lblPassword = new System.Windows.Forms.Label();
           this.updateTimer = new System.Windows.Forms.Timer(this.components);
           this.tray = new System.Windows.Forms.NotifyIcon(this.components);
           this.menu = new System.Windows.Forms.ContextMenuStrip(this.components);
           this.logInToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
           this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
           this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
           this.projectsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
           this.casesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
           this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
           this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
           this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
           this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
           this.recentCasesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
           this.tbManualCase = new System.Windows.Forms.ToolStripTextBox();
           this.stopWorkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
           this.btnOk = new System.Windows.Forms.Button();
           this.btnCancel = new System.Windows.Forms.Button();
           this.cbSSL = new System.Windows.Forms.CheckBox();
           this.menu.SuspendLayout();
           this.SuspendLayout();
           // 
           // tbServer
           // 
           this.tbServer.Location = new System.Drawing.Point(136, 12);
           this.tbServer.Name = "tbServer";
           this.tbServer.Size = new System.Drawing.Size(158, 21);
           this.tbServer.TabIndex = 0;
           // 
           // tbUser
           // 
           this.tbUser.Location = new System.Drawing.Point(136, 62);
           this.tbUser.Name = "tbUser";
           this.tbUser.Size = new System.Drawing.Size(158, 21);
           this.tbUser.TabIndex = 1;
           // 
           // tbPassword
           // 
           this.tbPassword.Location = new System.Drawing.Point(136, 88);
           this.tbPassword.Name = "tbPassword";
           this.tbPassword.Size = new System.Drawing.Size(158, 21);
           this.tbPassword.TabIndex = 2;
           this.tbPassword.UseSystemPasswordChar = true;
           // 
           // lblServer
           // 
           this.lblServer.AutoSize = true;
           this.lblServer.Location = new System.Drawing.Point(12, 15);
           this.lblServer.Name = "lblServer";
           this.lblServer.Size = new System.Drawing.Size(87, 13);
           this.lblServer.TabIndex = 3;
           this.lblServer.Text = "FogBugz Server:";
           // 
           // lblUser
           // 
           this.lblUser.AutoSize = true;
           this.lblUser.Location = new System.Drawing.Point(12, 65);
           this.lblUser.Name = "lblUser";
           this.lblUser.Size = new System.Drawing.Size(39, 13);
           this.lblUser.TabIndex = 4;
           this.lblUser.Text = "E-Mail:";
           // 
           // lblPassword
           // 
           this.lblPassword.AutoSize = true;
           this.lblPassword.Location = new System.Drawing.Point(12, 91);
           this.lblPassword.Name = "lblPassword";
           this.lblPassword.Size = new System.Drawing.Size(57, 13);
           this.lblPassword.TabIndex = 5;
           this.lblPassword.Text = "Password:";
           // 
           // updateTimer
           // 
           this.updateTimer.Interval = 1800000;
           this.updateTimer.Tick += new System.EventHandler(this.updateTimer_Tick);
           // 
           // tray
           // 
           this.tray.ContextMenuStrip = this.menu;
           this.tray.Icon = ((System.Drawing.Icon)(resources.GetObject("tray.Icon")));
           this.tray.Text = "FogBugz";
           this.tray.Visible = true;
           this.tray.MouseClick += new System.Windows.Forms.MouseEventHandler(this.tray_MouseClick);
           this.tray.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.tray_MouseDoubleClick);
           // 
           // menu
           // 
           this.menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.logInToolStripMenuItem,
            this.aboutToolStripMenuItem,
            this.toolStripSeparator1,
            this.projectsToolStripMenuItem,
            this.casesToolStripMenuItem,
            this.refreshToolStripMenuItem,
            this.toolStripSeparator2,
            this.exitToolStripMenuItem,
            this.toolStripSeparator3,
            this.recentCasesToolStripMenuItem,
            this.tbManualCase,
            this.stopWorkToolStripMenuItem});
           this.menu.Name = "menu";
           this.menu.Size = new System.Drawing.Size(161, 223);
           this.menu.Click += new System.EventHandler(this.menu_Click);
           // 
           // logInToolStripMenuItem
           // 
           this.logInToolStripMenuItem.Name = "logInToolStripMenuItem";
           this.logInToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
           this.logInToolStripMenuItem.Text = "&Log In...";
           this.logInToolStripMenuItem.Click += new System.EventHandler(this.logInToolStripMenuItem_Click);
           // 
           // aboutToolStripMenuItem
           // 
           this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
           this.aboutToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
           this.aboutToolStripMenuItem.Text = "&About...";
           this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
           // 
           // toolStripSeparator1
           // 
           this.toolStripSeparator1.Name = "toolStripSeparator1";
           this.toolStripSeparator1.Size = new System.Drawing.Size(157, 6);
           // 
           // projectsToolStripMenuItem
           // 
           this.projectsToolStripMenuItem.Name = "projectsToolStripMenuItem";
           this.projectsToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
           this.projectsToolStripMenuItem.Text = "&Projects";
           // 
           // casesToolStripMenuItem
           // 
           this.casesToolStripMenuItem.Name = "casesToolStripMenuItem";
           this.casesToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
           this.casesToolStripMenuItem.Text = "&Cases";
           // 
           // refreshToolStripMenuItem
           // 
           this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
           this.refreshToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
           this.refreshToolStripMenuItem.Text = "&Refresh";
           this.refreshToolStripMenuItem.Click += new System.EventHandler(this.refreshToolStripMenuItem_Click);
           // 
           // toolStripSeparator2
           // 
           this.toolStripSeparator2.Name = "toolStripSeparator2";
           this.toolStripSeparator2.Size = new System.Drawing.Size(157, 6);
           // 
           // exitToolStripMenuItem
           // 
           this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
           this.exitToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
           this.exitToolStripMenuItem.Text = "E&xit";
           this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
           // 
           // toolStripSeparator3
           // 
           this.toolStripSeparator3.Name = "toolStripSeparator3";
           this.toolStripSeparator3.Size = new System.Drawing.Size(157, 6);
           // 
           // recentCasesToolStripMenuItem
           // 
           this.recentCasesToolStripMenuItem.Name = "recentCasesToolStripMenuItem";
           this.recentCasesToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
           this.recentCasesToolStripMenuItem.Text = "Recent Cases";
           // 
           // tbManualCase
           // 
           this.tbManualCase.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
           this.tbManualCase.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
           this.tbManualCase.ForeColor = System.Drawing.Color.Gray;
           this.tbManualCase.Name = "tbManualCase";
           this.tbManualCase.Size = new System.Drawing.Size(100, 23);
           this.tbManualCase.Text = "Case #";
           this.tbManualCase.Enter += new System.EventHandler(this.tbManualCase_Enter);
           this.tbManualCase.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbManualCase_KeyPress);
           this.tbManualCase.Click += new System.EventHandler(this.tbManualCase_Enter);
           // 
           // stopWorkToolStripMenuItem
           // 
           this.stopWorkToolStripMenuItem.Enabled = false;
           this.stopWorkToolStripMenuItem.Name = "stopWorkToolStripMenuItem";
           this.stopWorkToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
           this.stopWorkToolStripMenuItem.Text = "&Stop Work";
           this.stopWorkToolStripMenuItem.Click += new System.EventHandler(this.stopWorkToolStripMenuItem_Click);
           // 
           // btnOk
           // 
           this.btnOk.Location = new System.Drawing.Point(138, 120);
           this.btnOk.Name = "btnOk";
           this.btnOk.Size = new System.Drawing.Size(75, 23);
           this.btnOk.TabIndex = 6;
           this.btnOk.Text = "OK";
           this.btnOk.UseVisualStyleBackColor = true;
           this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
           // 
           // btnCancel
           // 
           this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
           this.btnCancel.Location = new System.Drawing.Point(219, 120);
           this.btnCancel.Name = "btnCancel";
           this.btnCancel.Size = new System.Drawing.Size(75, 23);
           this.btnCancel.TabIndex = 6;
           this.btnCancel.Text = "Cancel";
           this.btnCancel.UseVisualStyleBackColor = true;
           this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
           // 
           // cbSSL
           // 
           this.cbSSL.AutoSize = true;
           this.cbSSL.Location = new System.Drawing.Point(136, 39);
           this.cbSSL.Name = "cbSSL";
           this.cbSSL.Size = new System.Drawing.Size(112, 17);
           this.cbSSL.TabIndex = 7;
           this.cbSSL.Text = "Use SSL (https://)";
           this.cbSSL.UseVisualStyleBackColor = true;
           // 
           // MainForm
           // 
           this.AcceptButton = this.btnOk;
           this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
           this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
           this.CancelButton = this.btnCancel;
           this.ClientSize = new System.Drawing.Size(305, 155);
           this.Controls.Add(this.cbSSL);
           this.Controls.Add(this.lblPassword);
           this.Controls.Add(this.lblUser);
           this.Controls.Add(this.lblServer);
           this.Controls.Add(this.tbPassword);
           this.Controls.Add(this.tbUser);
           this.Controls.Add(this.btnCancel);
           this.Controls.Add(this.btnOk);
           this.Controls.Add(this.tbServer);
           this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
           this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
           this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
           this.MaximizeBox = false;
           this.Name = "MainForm";
           this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
           this.Text = "Log In Info";
           this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
           this.menu.ResumeLayout(false);
           this.menu.PerformLayout();
           this.ResumeLayout(false);
           this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbServer;
        private System.Windows.Forms.TextBox tbUser;
        private System.Windows.Forms.TextBox tbPassword;
        private System.Windows.Forms.Label lblServer;
        private System.Windows.Forms.Label lblUser;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.Timer updateTimer;
        private System.Windows.Forms.NotifyIcon tray;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.ContextMenuStrip menu;
        private System.Windows.Forms.ToolStripMenuItem logInToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem projectsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem casesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripTextBox tbManualCase;
        private System.Windows.Forms.ToolStripMenuItem stopWorkToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckBox cbSSL;
        private System.Windows.Forms.ToolStripMenuItem recentCasesToolStripMenuItem;
    }
}


