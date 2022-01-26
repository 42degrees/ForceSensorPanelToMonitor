namespace ForceSensorPanelToMonitor
{
    partial class SettingsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsDialog));
            this.btnStart = new System.Windows.Forms.Button();
            this.cbMonitorList = new System.Windows.Forms.ComboBox();
            this.labelMonitorList = new System.Windows.Forms.Label();
            this.lblWindowClass = new System.Windows.Forms.Label();
            this.tbWindowClass = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.trayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.label2 = new System.Windows.Forms.Label();
            this.cbRefreshInterval = new System.Windows.Forms.ComboBox();
            this.lblMessage = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.chkRunAtStartup = new System.Windows.Forms.CheckBox();
            this.chkPreventOtherApplications = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(358, 88);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(84, 23);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "Refresh Now";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.BtnStart_Click);
            // 
            // cbMonitorList
            // 
            this.cbMonitorList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbMonitorList.FormattingEnabled = true;
            this.cbMonitorList.Location = new System.Drawing.Point(111, 10);
            this.cbMonitorList.Name = "cbMonitorList";
            this.cbMonitorList.Size = new System.Drawing.Size(241, 21);
            this.cbMonitorList.TabIndex = 1;
            // 
            // labelMonitorList
            // 
            this.labelMonitorList.AutoSize = true;
            this.labelMonitorList.Location = new System.Drawing.Point(13, 13);
            this.labelMonitorList.Name = "labelMonitorList";
            this.labelMonitorList.Size = new System.Drawing.Size(92, 13);
            this.labelMonitorList.TabIndex = 2;
            this.labelMonitorList.Text = "Choose a monitor:";
            // 
            // lblWindowClass
            // 
            this.lblWindowClass.AutoSize = true;
            this.lblWindowClass.Location = new System.Drawing.Point(29, 49);
            this.lblWindowClass.Name = "lblWindowClass";
            this.lblWindowClass.Size = new System.Drawing.Size(76, 13);
            this.lblWindowClass.TabIndex = 3;
            this.lblWindowClass.Text = "Window class:";
            // 
            // tbWindowClass
            // 
            this.tbWindowClass.Location = new System.Drawing.Point(111, 45);
            this.tbWindowClass.Name = "tbWindowClass";
            this.tbWindowClass.Size = new System.Drawing.Size(241, 20);
            this.tbWindowClass.TabIndex = 4;
            this.tbWindowClass.Text = "TForm_HWMonitoringSensorPanel";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(37, 88);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Refresh rate:";
            // 
            // btnClose
            // 
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(448, 88);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(84, 23);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "Exit";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // trayIcon
            // 
            this.trayIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.trayIcon.BalloonTipText = "Force Sensor Panel To Monitor Minimized.";
            this.trayIcon.BalloonTipTitle = "FSPTM";
            this.trayIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("trayIcon.Icon")));
            this.trayIcon.Text = "Force Sensor Panel Tool";
            this.trayIcon.Visible = true;
            this.trayIcon.Click += new System.EventHandler(this.TrayIcon_Click);
            this.trayIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.TrayIcon_MouseDoubleClick);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 182);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(460, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "After changing settings, hitting Refresh Now will cause those settings to be save" +
    "d (if successful).";
            // 
            // cbRefreshInterval
            // 
            this.cbRefreshInterval.DisplayMember = "itemText";
            this.cbRefreshInterval.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbRefreshInterval.FormattingEnabled = true;
            this.cbRefreshInterval.Location = new System.Drawing.Point(111, 88);
            this.cbRefreshInterval.Name = "cbRefreshInterval";
            this.cbRefreshInterval.Size = new System.Drawing.Size(241, 21);
            this.cbRefreshInterval.TabIndex = 6;
            this.cbRefreshInterval.ValueMember = "seconds";
            this.cbRefreshInterval.SelectedIndexChanged += new System.EventHandler(this.CbRefreshInterval_SelectedIndexChanged);
            // 
            // lblMessage
            // 
            this.lblMessage.AutoSize = true;
            this.lblMessage.Location = new System.Drawing.Point(377, 32);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(0, 13);
            this.lblMessage.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 198);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(380, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "The refresh rate does not control updating if the current display profile changes" +
    ".";
            // 
            // chkRunAtStartup
            // 
            this.chkRunAtStartup.AutoSize = true;
            this.chkRunAtStartup.Location = new System.Drawing.Point(16, 121);
            this.chkRunAtStartup.Name = "chkRunAtStartup";
            this.chkRunAtStartup.Size = new System.Drawing.Size(168, 17);
            this.chkRunAtStartup.TabIndex = 9;
            this.chkRunAtStartup.Text = "Start application with windows";
            this.chkRunAtStartup.UseVisualStyleBackColor = true;
            this.chkRunAtStartup.CheckedChanged += new System.EventHandler(this.ChkRunAtStartup_CheckedChanged);
            // 
            // chkPreventOtherApplications
            // 
            this.chkPreventOtherApplications.AutoSize = true;
            this.chkPreventOtherApplications.Location = new System.Drawing.Point(16, 144);
            this.chkPreventOtherApplications.Name = "chkPreventOtherApplications";
            this.chkPreventOtherApplications.Size = new System.Drawing.Size(229, 17);
            this.chkPreventOtherApplications.TabIndex = 9;
            this.chkPreventOtherApplications.Text = "Prevent applications from using this monitor";
            this.chkPreventOtherApplications.UseVisualStyleBackColor = true;
            this.chkPreventOtherApplications.CheckedChanged += new System.EventHandler(this.ChkRunAtStartup_CheckedChanged);
            // 
            // SettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(542, 231);
            this.Controls.Add(this.chkPreventOtherApplications);
            this.Controls.Add(this.chkRunAtStartup);
            this.Controls.Add(this.lblMessage);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cbRefreshInterval);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbWindowClass);
            this.Controls.Add(this.lblWindowClass);
            this.Controls.Add(this.labelMonitorList);
            this.Controls.Add(this.cbMonitorList);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnStart);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "SettingsDialog";
            this.Text = "Force sensor panel to stick to a single monitor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain1_FormClosing);
            this.Load += new System.EventHandler(this.FormMain1_Load);
            this.Resize += new System.EventHandler(this.FormMain1_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.ComboBox cbMonitorList;
        private System.Windows.Forms.Label labelMonitorList;
        private System.Windows.Forms.Label lblWindowClass;
        private System.Windows.Forms.TextBox tbWindowClass;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.NotifyIcon trayIcon;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.ComboBox cbRefreshInterval;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox chkRunAtStartup;
        private System.Windows.Forms.CheckBox chkPreventOtherApplications;
    }
}

