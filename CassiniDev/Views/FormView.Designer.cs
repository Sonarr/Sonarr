namespace CassiniDev
{
    partial class FormView 
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormView));
            this.RootUrlLinkLabel = new System.Windows.Forms.LinkLabel();
            this.ApplicationPathTextBox = new System.Windows.Forms.TextBox();
            this.VirtualPathTextBox = new System.Windows.Forms.TextBox();
            this.HostNameTextBox = new System.Windows.Forms.TextBox();
            this.LabelPhysicalPath = new System.Windows.Forms.Label();
            this.AddHostEntryCheckBox = new System.Windows.Forms.CheckBox();
            this.LabelVPath = new System.Windows.Forms.Label();
            this.ButtonBrowsePhysicalPath = new System.Windows.Forms.Button();
            this.LabelHostName = new System.Windows.Forms.Label();
            this.GroupBoxPort = new System.Windows.Forms.GroupBox();
            this.PortRangeEndTextBox = new System.Windows.Forms.NumericUpDown();
            this.PortRangeStartTextBox = new System.Windows.Forms.NumericUpDown();
            this.PortTextBox = new System.Windows.Forms.NumericUpDown();
            this.LabelPortRangeSeperator = new System.Windows.Forms.Label();
            this.PortModeFirstAvailableRadioButton = new System.Windows.Forms.RadioButton();
            this.PortModeSpecificRadioButton = new System.Windows.Forms.RadioButton();
            this.GroupBoxIPAddress = new System.Windows.Forms.GroupBox();
            this.IPSpecificTextBox = new System.Windows.Forms.ComboBox();
            this.IPV6CheckBox = new System.Windows.Forms.CheckBox();
            this.RadioButtonIPSpecific = new System.Windows.Forms.RadioButton();
            this.IPModeAnyRadioButton = new System.Windows.Forms.RadioButton();
            this.IPModeLoopBackRadioButton = new System.Windows.Forms.RadioButton();
            this.ButtonStart = new System.Windows.Forms.Button();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.LabelIdleTimeOut = new System.Windows.Forms.Label();
            this.TrayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.showToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.browseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.ShowLogMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.ShowLogButton = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hideToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nTLMAuthenticationRequiredToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.directoryBrowsingEnabledToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.TimeOutNumeric = new System.Windows.Forms.NumericUpDown();
            this.GroupBoxPort.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PortRangeEndTextBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PortRangeStartTextBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PortTextBox)).BeginInit();
            this.GroupBoxIPAddress.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TimeOutNumeric)).BeginInit();
            this.SuspendLayout();
            // 
            // RootUrlLinkLabel
            // 
            this.RootUrlLinkLabel.AutoSize = true;
            this.RootUrlLinkLabel.Location = new System.Drawing.Point(9, 300);
            this.RootUrlLinkLabel.Name = "RootUrlLinkLabel";
            this.RootUrlLinkLabel.Size = new System.Drawing.Size(189, 13);
            this.RootUrlLinkLabel.TabIndex = 24;
            this.RootUrlLinkLabel.TabStop = true;
            this.RootUrlLinkLabel.Text = "XXXXXXXXXXXXXXXXXXXXXXXXXX";
            this.RootUrlLinkLabel.Visible = false;
            this.RootUrlLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LaunchBrowser);
            // 
            // ApplicationPathTextBox
            // 
            this.ApplicationPathTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.ApplicationPathTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
            this.ApplicationPathTextBox.Location = new System.Drawing.Point(12, 53);
            this.ApplicationPathTextBox.Name = "ApplicationPathTextBox";
            this.ApplicationPathTextBox.Size = new System.Drawing.Size(285, 20);
            this.ApplicationPathTextBox.TabIndex = 0;
            this.toolTip1.SetToolTip(this.ApplicationPathTextBox, "The physical directory of the web application or site to serve.");
            // 
            // VirtualPathTextBox
            // 
            this.VirtualPathTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.VirtualPathTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.RecentlyUsedList;
            this.VirtualPathTextBox.Location = new System.Drawing.Point(12, 92);
            this.VirtualPathTextBox.Name = "VirtualPathTextBox";
            this.VirtualPathTextBox.Size = new System.Drawing.Size(317, 20);
            this.VirtualPathTextBox.TabIndex = 2;
            this.VirtualPathTextBox.Text = "/";
            this.toolTip1.SetToolTip(this.VirtualPathTextBox, "The virtual path upon which to root the application.");
            // 
            // HostNameTextBox
            // 
            this.HostNameTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.HostNameTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.RecentlyUsedList;
            this.HostNameTextBox.Location = new System.Drawing.Point(12, 131);
            this.HostNameTextBox.Name = "HostNameTextBox";
            this.HostNameTextBox.Size = new System.Drawing.Size(213, 20);
            this.HostNameTextBox.TabIndex = 3;
            this.toolTip1.SetToolTip(this.HostNameTextBox, resources.GetString("HostNameTextBox.ToolTip"));
            this.HostNameTextBox.TextChanged += new System.EventHandler(this.HostNameChanged);
            // 
            // LabelPhysicalPath
            // 
            this.LabelPhysicalPath.AutoSize = true;
            this.LabelPhysicalPath.Location = new System.Drawing.Point(12, 36);
            this.LabelPhysicalPath.Name = "LabelPhysicalPath";
            this.LabelPhysicalPath.Size = new System.Drawing.Size(71, 13);
            this.LabelPhysicalPath.TabIndex = 1;
            this.LabelPhysicalPath.Text = "Physical Path";
            this.toolTip1.SetToolTip(this.LabelPhysicalPath, "The physical directory of the web application or site to serve.");
            // 
            // AddHostEntryCheckBox
            // 
            this.AddHostEntryCheckBox.AutoSize = true;
            this.AddHostEntryCheckBox.Enabled = false;
            this.AddHostEntryCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.AddHostEntryCheckBox.Location = new System.Drawing.Point(231, 133);
            this.AddHostEntryCheckBox.Name = "AddHostEntryCheckBox";
            this.AddHostEntryCheckBox.Size = new System.Drawing.Size(105, 18);
            this.AddHostEntryCheckBox.TabIndex = 4;
            this.AddHostEntryCheckBox.Text = "Add hosts entry";
            this.toolTip1.SetToolTip(this.AddHostEntryCheckBox, resources.GetString("AddHostEntryCheckBox.ToolTip"));
            this.AddHostEntryCheckBox.UseVisualStyleBackColor = true;
            // 
            // LabelVPath
            // 
            this.LabelVPath.AutoSize = true;
            this.LabelVPath.Location = new System.Drawing.Point(12, 75);
            this.LabelVPath.Name = "LabelVPath";
            this.LabelVPath.Size = new System.Drawing.Size(61, 13);
            this.LabelVPath.TabIndex = 3;
            this.LabelVPath.Text = "Virtual Path";
            this.toolTip1.SetToolTip(this.LabelVPath, "The virtual path upon which to root the application.");
            // 
            // ButtonBrowsePhysicalPath
            // 
            this.ButtonBrowsePhysicalPath.Location = new System.Drawing.Point(303, 50);
            this.ButtonBrowsePhysicalPath.Name = "ButtonBrowsePhysicalPath";
            this.ButtonBrowsePhysicalPath.Size = new System.Drawing.Size(27, 23);
            this.ButtonBrowsePhysicalPath.TabIndex = 1;
            this.ButtonBrowsePhysicalPath.Text = "...";
            this.toolTip1.SetToolTip(this.ButtonBrowsePhysicalPath, "Browse");
            this.ButtonBrowsePhysicalPath.UseVisualStyleBackColor = true;
            this.ButtonBrowsePhysicalPath.Click += new System.EventHandler(this.BrowsePath);
            // 
            // LabelHostName
            // 
            this.LabelHostName.AutoSize = true;
            this.LabelHostName.Location = new System.Drawing.Point(12, 115);
            this.LabelHostName.Name = "LabelHostName";
            this.LabelHostName.Size = new System.Drawing.Size(106, 13);
            this.LabelHostName.TabIndex = 5;
            this.LabelHostName.Text = "Host Name (optional)";
            this.toolTip1.SetToolTip(this.LabelHostName, resources.GetString("LabelHostName.ToolTip"));
            // 
            // GroupBoxPort
            // 
            this.GroupBoxPort.Controls.Add(this.PortRangeEndTextBox);
            this.GroupBoxPort.Controls.Add(this.PortRangeStartTextBox);
            this.GroupBoxPort.Controls.Add(this.PortTextBox);
            this.GroupBoxPort.Controls.Add(this.LabelPortRangeSeperator);
            this.GroupBoxPort.Controls.Add(this.PortModeFirstAvailableRadioButton);
            this.GroupBoxPort.Controls.Add(this.PortModeSpecificRadioButton);
            this.GroupBoxPort.Location = new System.Drawing.Point(12, 213);
            this.GroupBoxPort.Name = "GroupBoxPort";
            this.GroupBoxPort.Size = new System.Drawing.Size(324, 47);
            this.GroupBoxPort.TabIndex = 9;
            this.GroupBoxPort.TabStop = false;
            this.GroupBoxPort.Text = "Port";
            // 
            // PortRangeEndTextBox
            // 
            this.PortRangeEndTextBox.Location = new System.Drawing.Point(261, 12);
            this.PortRangeEndTextBox.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.PortRangeEndTextBox.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.PortRangeEndTextBox.Name = "PortRangeEndTextBox";
            this.PortRangeEndTextBox.Size = new System.Drawing.Size(57, 20);
            this.PortRangeEndTextBox.TabIndex = 19;
            this.PortRangeEndTextBox.Value = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            // 
            // PortRangeStartTextBox
            // 
            this.PortRangeStartTextBox.Location = new System.Drawing.Point(182, 12);
            this.PortRangeStartTextBox.Maximum = new decimal(new int[] {
            65534,
            0,
            0,
            0});
            this.PortRangeStartTextBox.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.PortRangeStartTextBox.Name = "PortRangeStartTextBox";
            this.PortRangeStartTextBox.Size = new System.Drawing.Size(57, 20);
            this.PortRangeStartTextBox.TabIndex = 18;
            this.PortRangeStartTextBox.Value = new decimal(new int[] {
            32768,
            0,
            0,
            0});
            // 
            // PortTextBox
            // 
            this.PortTextBox.Location = new System.Drawing.Point(65, 12);
            this.PortTextBox.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.PortTextBox.Name = "PortTextBox";
            this.PortTextBox.Size = new System.Drawing.Size(57, 20);
            this.PortTextBox.TabIndex = 17;
            // 
            // LabelPortRangeSeperator
            // 
            this.LabelPortRangeSeperator.AutoSize = true;
            this.LabelPortRangeSeperator.Location = new System.Drawing.Point(239, 16);
            this.LabelPortRangeSeperator.Name = "LabelPortRangeSeperator";
            this.LabelPortRangeSeperator.Size = new System.Drawing.Size(22, 13);
            this.LabelPortRangeSeperator.TabIndex = 16;
            this.LabelPortRangeSeperator.Text = "<->";
            this.toolTip1.SetToolTip(this.LabelPortRangeSeperator, "Host on the first available port found in specified range.");
            // 
            // PortModeFirstAvailableRadioButton
            // 
            this.PortModeFirstAvailableRadioButton.AutoSize = true;
            this.PortModeFirstAvailableRadioButton.Checked = true;
            this.PortModeFirstAvailableRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.PortModeFirstAvailableRadioButton.Location = new System.Drawing.Point(128, 13);
            this.PortModeFirstAvailableRadioButton.Name = "PortModeFirstAvailableRadioButton";
            this.PortModeFirstAvailableRadioButton.Size = new System.Drawing.Size(63, 18);
            this.PortModeFirstAvailableRadioButton.TabIndex = 6;
            this.PortModeFirstAvailableRadioButton.TabStop = true;
            this.PortModeFirstAvailableRadioButton.Text = "Range";
            this.toolTip1.SetToolTip(this.PortModeFirstAvailableRadioButton, "Host on the first available port found in specified range.");
            this.PortModeFirstAvailableRadioButton.UseVisualStyleBackColor = true;
            this.PortModeFirstAvailableRadioButton.CheckedChanged += new System.EventHandler(this.RadioButtonPortFind_CheckedChanged);
            // 
            // PortModeSpecificRadioButton
            // 
            this.PortModeSpecificRadioButton.AutoSize = true;
            this.PortModeSpecificRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.PortModeSpecificRadioButton.Location = new System.Drawing.Point(6, 13);
            this.PortModeSpecificRadioButton.Name = "PortModeSpecificRadioButton";
            this.PortModeSpecificRadioButton.Size = new System.Drawing.Size(69, 18);
            this.PortModeSpecificRadioButton.TabIndex = 6;
            this.PortModeSpecificRadioButton.TabStop = true;
            this.PortModeSpecificRadioButton.Text = "Specific";
            this.toolTip1.SetToolTip(this.PortModeSpecificRadioButton, "Host on specific port. \r\nIf port is already in use a warning will be issued and s" +
                    "erver will not start.");
            this.PortModeSpecificRadioButton.UseVisualStyleBackColor = true;
            this.PortModeSpecificRadioButton.CheckedChanged += new System.EventHandler(this.RadioButtonPortSpecific_CheckedChanged);
            // 
            // GroupBoxIPAddress
            // 
            this.GroupBoxIPAddress.Controls.Add(this.IPSpecificTextBox);
            this.GroupBoxIPAddress.Controls.Add(this.IPV6CheckBox);
            this.GroupBoxIPAddress.Controls.Add(this.RadioButtonIPSpecific);
            this.GroupBoxIPAddress.Controls.Add(this.IPModeAnyRadioButton);
            this.GroupBoxIPAddress.Controls.Add(this.IPModeLoopBackRadioButton);
            this.GroupBoxIPAddress.Location = new System.Drawing.Point(12, 154);
            this.GroupBoxIPAddress.Name = "GroupBoxIPAddress";
            this.GroupBoxIPAddress.Size = new System.Drawing.Size(324, 58);
            this.GroupBoxIPAddress.TabIndex = 8;
            this.GroupBoxIPAddress.TabStop = false;
            this.GroupBoxIPAddress.Text = "IP Address";
            // 
            // IPSpecificTextBox
            // 
            this.IPSpecificTextBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.IPSpecificTextBox.FormattingEnabled = true;
            this.IPSpecificTextBox.Location = new System.Drawing.Point(216, 14);
            this.IPSpecificTextBox.Name = "IPSpecificTextBox";
            this.IPSpecificTextBox.Size = new System.Drawing.Size(101, 21);
            this.IPSpecificTextBox.TabIndex = 9;
            // 
            // IPV6CheckBox
            // 
            this.IPV6CheckBox.AutoSize = true;
            this.IPV6CheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.IPV6CheckBox.Location = new System.Drawing.Point(39, 35);
            this.IPV6CheckBox.Name = "IPV6CheckBox";
            this.IPV6CheckBox.Size = new System.Drawing.Size(77, 18);
            this.IPV6CheckBox.TabIndex = 8;
            this.IPV6CheckBox.Text = "Use IPV6";
            this.toolTip1.SetToolTip(this.IPV6CheckBox, "Use the IPV6 version of selected IP");
            this.IPV6CheckBox.UseVisualStyleBackColor = true;
            // 
            // RadioButtonIPSpecific
            // 
            this.RadioButtonIPSpecific.AutoSize = true;
            this.RadioButtonIPSpecific.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.RadioButtonIPSpecific.Location = new System.Drawing.Point(150, 15);
            this.RadioButtonIPSpecific.Name = "RadioButtonIPSpecific";
            this.RadioButtonIPSpecific.Size = new System.Drawing.Size(69, 18);
            this.RadioButtonIPSpecific.TabIndex = 5;
            this.RadioButtonIPSpecific.Text = "Specific";
            this.toolTip1.SetToolTip(this.RadioButtonIPSpecific, "Host on specified IP address.\r\nWCF Services may not be served using this setting." +
                    " Use Loopback.");
            this.RadioButtonIPSpecific.UseVisualStyleBackColor = true;
            this.RadioButtonIPSpecific.CheckedChanged += new System.EventHandler(this.RadioButtonIPSpecific_CheckedChanged);
            // 
            // IPModeAnyRadioButton
            // 
            this.IPModeAnyRadioButton.AutoSize = true;
            this.IPModeAnyRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.IPModeAnyRadioButton.Location = new System.Drawing.Point(88, 15);
            this.IPModeAnyRadioButton.Name = "IPModeAnyRadioButton";
            this.IPModeAnyRadioButton.Size = new System.Drawing.Size(49, 18);
            this.IPModeAnyRadioButton.TabIndex = 5;
            this.IPModeAnyRadioButton.Text = "Any";
            this.toolTip1.SetToolTip(this.IPModeAnyRadioButton, "Host on all IP addresses at the specified port.\r\nWCF Services may not be served u" +
                    "sing this setting. Use Loopback.");
            this.IPModeAnyRadioButton.UseVisualStyleBackColor = true;
            this.IPModeAnyRadioButton.CheckedChanged += new System.EventHandler(this.RadioButtonIPAny_CheckedChanged);
            // 
            // IPModeLoopBackRadioButton
            // 
            this.IPModeLoopBackRadioButton.AutoSize = true;
            this.IPModeLoopBackRadioButton.Checked = true;
            this.IPModeLoopBackRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.IPModeLoopBackRadioButton.Location = new System.Drawing.Point(6, 15);
            this.IPModeLoopBackRadioButton.Name = "IPModeLoopBackRadioButton";
            this.IPModeLoopBackRadioButton.Size = new System.Drawing.Size(79, 18);
            this.IPModeLoopBackRadioButton.TabIndex = 5;
            this.IPModeLoopBackRadioButton.TabStop = true;
            this.IPModeLoopBackRadioButton.Text = "Loopback";
            this.toolTip1.SetToolTip(this.IPModeLoopBackRadioButton, "Use the default loopback adapter. \r\nIf any sort of WCF service is to be served, t" +
                    "his is the only viable option.");
            this.IPModeLoopBackRadioButton.UseVisualStyleBackColor = true;
            this.IPModeLoopBackRadioButton.CheckedChanged += new System.EventHandler(this.RadioButtonIPLoopBack_CheckedChanged);
            // 
            // ButtonStart
            // 
            this.ButtonStart.Location = new System.Drawing.Point(261, 335);
            this.ButtonStart.Name = "ButtonStart";
            this.ButtonStart.Size = new System.Drawing.Size(75, 23);
            this.ButtonStart.TabIndex = 22;
            this.ButtonStart.Text = "Start";
            this.toolTip1.SetToolTip(this.ButtonStart, "Start hosting application using specified criteria");
            this.ButtonStart.UseVisualStyleBackColor = true;
            this.ButtonStart.Click += new System.EventHandler(this.StartStop);
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // LabelIdleTimeOut
            // 
            this.LabelIdleTimeOut.AutoSize = true;
            this.LabelIdleTimeOut.Location = new System.Drawing.Point(15, 270);
            this.LabelIdleTimeOut.Name = "LabelIdleTimeOut";
            this.LabelIdleTimeOut.Size = new System.Drawing.Size(73, 13);
            this.LabelIdleTimeOut.TabIndex = 26;
            this.LabelIdleTimeOut.Text = "Idle Time Out:";
            this.toolTip1.SetToolTip(this.LabelIdleTimeOut, "The amount of time, in milliseconds, to remain idle, i.e. no requests, before sto" +
                    "pping the server.");
            // 
            // TrayIcon
            // 
            this.TrayIcon.ContextMenuStrip = this.contextMenuStrip1;
            this.TrayIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("TrayIcon.Icon")));
            this.TrayIcon.Text = "notifyIcon1";
            this.TrayIcon.BalloonTipClicked += new System.EventHandler(this.ShowMainForm);
            this.TrayIcon.DoubleClick += new System.EventHandler(this.ShowMainForm);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showToolStripMenuItem,
            this.browseToolStripMenuItem,
            this.toolStripSeparator2,
            this.ShowLogMenuItem,
            this.toolStripSeparator1,
            this.closeToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(117, 126);
            // 
            // showToolStripMenuItem
            // 
            this.showToolStripMenuItem.Name = "showToolStripMenuItem";
            this.showToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.showToolStripMenuItem.Text = "&Show";
            this.showToolStripMenuItem.Click += new System.EventHandler(this.ShowMainForm);
            // 
            // browseToolStripMenuItem
            // 
            this.browseToolStripMenuItem.Name = "browseToolStripMenuItem";
            this.browseToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.browseToolStripMenuItem.Text = "&Browse";
            this.browseToolStripMenuItem.Click += new System.EventHandler(this.LaunchBrowser);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(113, 6);
            // 
            // ShowLogMenuItem
            // 
            this.ShowLogMenuItem.Enabled = false;
            this.ShowLogMenuItem.Name = "ShowLogMenuItem";
            this.ShowLogMenuItem.Size = new System.Drawing.Size(116, 22);
            this.ShowLogMenuItem.Text = "&View Log";
            this.ShowLogMenuItem.Click += new System.EventHandler(this.ShowLog);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(113, 6);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.closeToolStripMenuItem.Text = "&Exit";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.ExitApp);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.helpToolStripMenuItem.Text = "&Help";
            this.helpToolStripMenuItem.Click += new System.EventHandler(this.ShowHelp);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 370);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.statusStrip1.Size = new System.Drawing.Size(343, 22);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 27;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(118, 17);
            this.toolStripStatusLabel1.Text = "ASP.Net Version XXXXX";
            // 
            // ShowLogButton
            // 
            this.ShowLogButton.Location = new System.Drawing.Point(176, 335);
            this.ShowLogButton.Margin = new System.Windows.Forms.Padding(0);
            this.ShowLogButton.Name = "ShowLogButton";
            this.ShowLogButton.Size = new System.Drawing.Size(75, 23);
            this.ShowLogButton.TabIndex = 28;
            this.ShowLogButton.Text = "Show &Log";
            this.ShowLogButton.Click += new System.EventHandler(this.ShowLog);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.helpToolStripMenuItem1});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(343, 24);
            this.menuStrip1.TabIndex = 29;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.hideToolStripMenuItem,
            this.toolStripSeparator4,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // hideToolStripMenuItem
            // 
            this.hideToolStripMenuItem.Name = "hideToolStripMenuItem";
            this.hideToolStripMenuItem.Size = new System.Drawing.Size(95, 22);
            this.hideToolStripMenuItem.Text = "&Hide";
            this.hideToolStripMenuItem.Click += new System.EventHandler(this.HideMainForm);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(92, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(95, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitApp);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nTLMAuthenticationRequiredToolStripMenuItem,
            this.directoryBrowsingEnabledToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
            this.optionsToolStripMenuItem.Text = "&Options";
            // 
            // nTLMAuthenticationRequiredToolStripMenuItem
            // 
            this.nTLMAuthenticationRequiredToolStripMenuItem.CheckOnClick = true;
            this.nTLMAuthenticationRequiredToolStripMenuItem.Name = "nTLMAuthenticationRequiredToolStripMenuItem";
            this.nTLMAuthenticationRequiredToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            this.nTLMAuthenticationRequiredToolStripMenuItem.Text = "NTLM Authentication Required";
            this.nTLMAuthenticationRequiredToolStripMenuItem.ToolTipText = "When checked, require windows authentication via NTLM.";
            // 
            // directoryBrowsingEnabledToolStripMenuItem
            // 
            this.directoryBrowsingEnabledToolStripMenuItem.Checked = true;
            this.directoryBrowsingEnabledToolStripMenuItem.CheckOnClick = true;
            this.directoryBrowsingEnabledToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.directoryBrowsingEnabledToolStripMenuItem.Name = "directoryBrowsingEnabledToolStripMenuItem";
            this.directoryBrowsingEnabledToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            this.directoryBrowsingEnabledToolStripMenuItem.Text = "Directory Browsing Enabled";
            this.directoryBrowsingEnabledToolStripMenuItem.ToolTipText = "When checked, if no default document is found display a directory listing.";
            // 
            // helpToolStripMenuItem1
            // 
            this.helpToolStripMenuItem1.Name = "helpToolStripMenuItem1";
            this.helpToolStripMenuItem1.Size = new System.Drawing.Size(40, 20);
            this.helpToolStripMenuItem1.Text = "&Help";
            this.helpToolStripMenuItem1.Click += new System.EventHandler(this.ShowHelp);
            // 
            // TimeOutNumeric
            // 
            this.TimeOutNumeric.Increment = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.TimeOutNumeric.Location = new System.Drawing.Point(95, 266);
            this.TimeOutNumeric.Maximum = new decimal(new int[] {
            2000000000,
            0,
            0,
            0});
            this.TimeOutNumeric.Name = "TimeOutNumeric";
            this.TimeOutNumeric.Size = new System.Drawing.Size(66, 20);
            this.TimeOutNumeric.TabIndex = 30;
            // 
            // FormView
            // 
            this.AcceptButton = this.ButtonStart;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(343, 392);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.TimeOutNumeric);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.LabelIdleTimeOut);
            this.Controls.Add(this.ApplicationPathTextBox);
            this.Controls.Add(this.ShowLogButton);
            this.Controls.Add(this.VirtualPathTextBox);
            this.Controls.Add(this.HostNameTextBox);
            this.Controls.Add(this.RootUrlLinkLabel);
            this.Controls.Add(this.LabelPhysicalPath);
            this.Controls.Add(this.LabelVPath);
            this.Controls.Add(this.GroupBoxIPAddress);
            this.Controls.Add(this.ButtonStart);
            this.Controls.Add(this.AddHostEntryCheckBox);
            this.Controls.Add(this.GroupBoxPort);
            this.Controls.Add(this.ButtonBrowsePhysicalPath);
            this.Controls.Add(this.LabelHostName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormView";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Cassini Developer Edition";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.GroupBoxPort.ResumeLayout(false);
            this.GroupBoxPort.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PortRangeEndTextBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PortRangeStartTextBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PortTextBox)).EndInit();
            this.GroupBoxIPAddress.ResumeLayout(false);
            this.GroupBoxIPAddress.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TimeOutNumeric)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.LinkLabel RootUrlLinkLabel;
        private System.Windows.Forms.TextBox ApplicationPathTextBox;
        private System.Windows.Forms.TextBox VirtualPathTextBox;
        private System.Windows.Forms.TextBox HostNameTextBox;
        private System.Windows.Forms.Label LabelPhysicalPath;
        private System.Windows.Forms.CheckBox AddHostEntryCheckBox;
        private System.Windows.Forms.Label LabelVPath;
        private System.Windows.Forms.Button ButtonBrowsePhysicalPath;
        private System.Windows.Forms.Label LabelHostName;
        private System.Windows.Forms.GroupBox GroupBoxPort;
        private System.Windows.Forms.Label LabelPortRangeSeperator;
        private System.Windows.Forms.RadioButton PortModeFirstAvailableRadioButton;
        private System.Windows.Forms.RadioButton PortModeSpecificRadioButton;
        private System.Windows.Forms.GroupBox GroupBoxIPAddress;
        private System.Windows.Forms.RadioButton RadioButtonIPSpecific;
        private System.Windows.Forms.RadioButton IPModeAnyRadioButton;
        private System.Windows.Forms.RadioButton IPModeLoopBackRadioButton;
        private System.Windows.Forms.Button ButtonStart;
        private System.Windows.Forms.CheckBox IPV6CheckBox;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label LabelIdleTimeOut;
        internal System.Windows.Forms.NotifyIcon TrayIcon;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem showToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem browseToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem ShowLogMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.Button ShowLogButton;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hideToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem nTLMAuthenticationRequiredToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem directoryBrowsingEnabledToolStripMenuItem;
        private System.Windows.Forms.NumericUpDown TimeOutNumeric;
        private System.Windows.Forms.NumericUpDown PortTextBox;
        private System.Windows.Forms.NumericUpDown PortRangeStartTextBox;
        private System.Windows.Forms.NumericUpDown PortRangeEndTextBox;
        private System.Windows.Forms.ComboBox IPSpecificTextBox;
    }
}