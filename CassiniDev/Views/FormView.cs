//  **********************************************************************************
//  CassiniDev - http://cassinidev.codeplex.com
// 
//  Copyright (c) Sky Sanders. All rights reserved.
//  
//  This source code is subject to terms and conditions of the Microsoft Public
//  License (Ms-PL). A copy of the license can be found in the license.htm file
//  included in this distribution.
//  
//  You must not remove this notice, or any other, from this software.
//  
//  **********************************************************************************

#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using CassiniDev.ServerLog;

#endregion

namespace CassiniDev
{
    public partial class FormView : Form
    {
        #region Fields

        private bool _automated;

        private LogView _logForm;

        private RunState _runState;

        private Server _server;

        #endregion

        #region Constructors

        public FormView(Server server)
        {
            _server = server;
            InitializeComponent();
            InitializeUI();
        }

        #endregion

        #region Properties

        internal bool AddHost
        {
            get { return AddHostEntryCheckBox.Checked; }
            set { AddHostEntryCheckBox.Checked = value; }
        }

        internal string ApplicationPath
        {
            get { return ApplicationPathTextBox.Text; }
            set { ApplicationPathTextBox.Text = value; }
        }

        internal string HostName
        {
            get { return HostNameTextBox.Text; }
            set { HostNameTextBox.Text = value; }
        }

        internal string IPAddress
        {
            get { return IPSpecificTextBox.Text; }
            set { IPSpecificTextBox.Text = value; }
        }

        internal IPMode IPMode
        {
            get { return GetIpMode(); }
            set { SetIpMode(value); }
        }


        internal bool NoDirList
        {
            get { return !directoryBrowsingEnabledToolStripMenuItem.Checked; }
            set { directoryBrowsingEnabledToolStripMenuItem.Checked = !value; }
        }

        internal bool NtmlAuthenticationRequired
        {
            get { return nTLMAuthenticationRequiredToolStripMenuItem.Checked; }
            set { nTLMAuthenticationRequiredToolStripMenuItem.Checked = value; }
        }

        internal int Port
        {
            get { return (int)PortTextBox.Value; }
            set { PortTextBox.Value = value; }
        }

        internal PortMode PortMode
        {
            get { return GetPortMode(); }
            set { SetPortMode(value); }
        }

        internal int PortRangeEnd
        {
            get { return (int)PortRangeEndTextBox.Value; }
            set { PortRangeEndTextBox.Value = value; }
        }

        internal int PortRangeStart
        {
            get { return (int)PortRangeStartTextBox.Value; }
            set { PortRangeStartTextBox.Value = value; }
        }

        internal string RootUrl
        {
            get { return RootUrlLinkLabel.Text; }
            set
            {
                RootUrlLinkLabel.Text = value;
                RootUrlLinkLabel.Visible = !string.IsNullOrEmpty(value);
            }
        }

        internal RunState RunState
        {
            get { return _runState; }
        }

        internal int TimeOut
        {
            get { return (int)TimeOutNumeric.Value; }
            set { TimeOutNumeric.Value = value; }
        }

        internal bool V6
        {
            get { return IPV6CheckBox.Checked; }
            set { IPV6CheckBox.Checked = value; }
        }

        internal string VirtualPath
        {
            get { return VirtualPathTextBox.Text; }
            set { VirtualPathTextBox.Text = value; }
        }

        #endregion

        #region Protected Methods

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            if (disposing && _server != null)
            {
                _server.Dispose();
                _server = null;
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// If the form is closing we need to determine whether to exit
        /// or to minimize to tray.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            // if explicit closure or we are doing nothing then exit app.
            if (RunState == RunState.Idle)
            {
                InvokeStop();
            }
            else
            {
                WindowState = FormWindowState.Minimized;
                e.Cancel = true;
            }
        }

        //protected override void OnShown(EventArgs e)
        //{
        //    if (_automated)
        //    {
        //        WindowState = FormWindowState.Minimized;
        //        Hide();
        //    }
        //}

        #endregion

        #region Private Methods


        private void InitializeUI()
        {
            ButtonStart.Text = SR.GetString(SR.WebdevStart);
            toolStripStatusLabel1.Text = SR.GetString(SR.WebdevAspNetVersion, Common.GetAspVersion());

            
            // if sqlite is missing then just silently enable in-memory logging,
            // hide the enable logging item and enable view log item
            
            
            
            ShowLogButton.Enabled = false;

            

            toolStripStatusLabel1.Text = SR.GetString(SR.WebdevAspNetVersion, Common.GetAspVersion());

            List<IPAddress> localAddresses = new List<IPAddress>(CassiniNetworkUtils.GetLocalAddresses());
            localAddresses.Insert(0, System.Net.IPAddress.IPv6Loopback);
            localAddresses.Insert(0, System.Net.IPAddress.Loopback);

            IPSpecificTextBox.Items.AddRange(localAddresses.Select(i => i.ToString()).ToArray());
            if (IPSpecificTextBox.Items.Count > 0)
            {
                IPSpecificTextBox.SelectedIndex = 0;
                IPSpecificTextBox.Text = IPSpecificTextBox.Items[0].ToString();
            }

            InvokeSetRunState(RunState.Idle);
            if (_server == null)
            {
                ShowMainForm();
            }
            else
            {
                _automated = true;
                _server.TimedOut += OnTimedOut;
                IPMode = IPMode.Specific;
                PortMode = PortMode.Specific;

                UpdateUIFromServer();


                InvokeSetRunState(RunState.Running);
                base.Text = SR.GetString(SR.WebdevNameWithPort, _server.Port);
                TrayIcon.Visible = true;
                DisplayTrayTip();
            }
        }

        private void DisplayTrayTip()
        {

            if (_server==null)
            {
                return;
            }
            TrayIcon.Text = _server.RootUrl;
            string trayBaloonText = _server.RootUrl;
            TrayIcon.ShowBalloonTip(5000, base.Text, trayBaloonText, ToolTipIcon.Info);
        }

        private void UpdateUIFromServer()
        {
            base.Text = SR.GetString(SR.WebdevNameWithPort, _server.Port);
            RootUrl = _server.RootUrl;
            ApplicationPath = _server.PhysicalPath;
            IPAddress = _server.IPAddress.ToString();
            Port = _server.Port;
            HostName = _server.HostName;
            NtmlAuthenticationRequired = _server.RequireAuthentication;
            NoDirList = _server.DisableDirectoryListing;
            TimeOut = _server.TimeoutInterval;
        }

        protected override void OnResize(EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                DisplayTrayTip();
                ShowInTaskbar = false;
            }
            base.OnResize(e);
        }


        private void ShowMainForm()
        {
            Show();
            WindowState = FormWindowState.Normal;
            ShowInTaskbar = true;


            //TrayIcon.Visible = false;
            //ShowInTaskbar = true;
            
            //TopMost = true;
            //Focus();
            //BringToFront();
            //TopMost = false;
        }

        private void InvokeSetRunState(RunState state)
        {
            // use invoke, runstate may come from another thread.
            if (InvokeRequired)
            {
                Invoke(new ParameterizedThreadStart(SetRunState), state);
            }
            else
            {
                SetRunState(state);
            }
        }

        /// <summary>
        /// Sets RunState and enables/disables form fields accordingly
        /// </summary>
        /// <param name="state"></param>
        private void SetRunState(object state)
        {
            _runState = (RunState)state;

            switch (_runState)
            {
                case RunState.Idle:
                    if (!_automated)
                    {
                        EnableForm();
                    }
                    // if not automated we are on our way out
                    break;
                case RunState.Running:
                    DisableForm();
                    break;
            }
        }




        private void EnableForm()
        {
            ShowLogMenuItem.Enabled = ShowLogButton.Enabled = false;
            base.Text = SR.GetString(SR.WebdevName);
            ButtonStart.Text = "&Start";
            ButtonStart.Enabled = true;
            nTLMAuthenticationRequiredToolStripMenuItem.Enabled = true;
            directoryBrowsingEnabledToolStripMenuItem.Enabled = true;
            ApplicationPathTextBox.Enabled = true;
            ButtonBrowsePhysicalPath.Enabled = true;
            VirtualPathTextBox.Enabled = true;
            HostNameTextBox.Enabled = true;
            GroupBoxIPAddress.Enabled = true;
            GroupBoxPort.Enabled = true;
            LabelHostName.Enabled = true;
            LabelPhysicalPath.Enabled = true;
            LabelVPath.Enabled = true;
            TimeOutNumeric.Enabled = true;
            RootUrl = null;

            AddHostEntryCheckBox.Enabled = !String.IsNullOrEmpty(HostName);

            switch (IPMode)
            {
                case IPMode.Loopback:
                    RadioButtonIPLoopBack_CheckedChanged(null, EventArgs.Empty);
                    break;
                case IPMode.Any:
                    RadioButtonIPAny_CheckedChanged(null, EventArgs.Empty);
                    break;
                case IPMode.Specific:
                    RadioButtonIPSpecific_CheckedChanged(null, EventArgs.Empty);
                    break;
            }

            switch (PortMode)
            {
                case PortMode.FirstAvailable:
                    RadioButtonPortFind_CheckedChanged(null, EventArgs.Empty);
                    break;
                case PortMode.Specific:
                    RadioButtonPortSpecific_CheckedChanged(null, EventArgs.Empty);
                    break;
            }

            HostNameChanged(null, EventArgs.Empty);
        }

        private void DisableForm()
        {
            ShowLogMenuItem.Enabled = ShowLogButton.Enabled = true;

            TimeOutNumeric.Enabled = false;
            ButtonStart.Text = "&Stop";
            directoryBrowsingEnabledToolStripMenuItem.Enabled = false;
            nTLMAuthenticationRequiredToolStripMenuItem.Enabled = false;
            ApplicationPathTextBox.Enabled = false;
            ButtonBrowsePhysicalPath.Enabled = false;
            VirtualPathTextBox.Enabled = false;
            HostNameTextBox.Enabled = false;
            GroupBoxIPAddress.Enabled = false;
            GroupBoxPort.Enabled = false;
            AddHostEntryCheckBox.Enabled = false;
            LabelHostName.Enabled = false;
            LabelPhysicalPath.Enabled = false;
            LabelVPath.Enabled = false;
        }

        private void HostNameChanged()
        {
            if (string.IsNullOrEmpty(HostName))
            {
                AddHostEntryCheckBox.Enabled = false;
                AddHost = false;
            }
            else
            {
                AddHostEntryCheckBox.Enabled = true;
            }
        }


        private void StartStop()
        {
            if (RunState != RunState.Running)
            {
                DisableForm();
                Start();
            }
            else
            {
                InvokeStop();
            }
        }

        private CommandLineArguments GetArgs()
        {
            CommandLineArguments args = new CommandLineArguments
                        {
                            AddHost = AddHost,
                            ApplicationPath = ApplicationPath,
                            HostName = HostName,
                            IPAddress = IPAddress,
                            IPMode = IPMode,
                            IPv6 = V6,
                            Port = Port,
                            PortMode = PortMode,
                            PortRangeEnd = PortRangeEnd,
                            PortRangeStart = PortRangeStart,
                            VirtualPath = VirtualPath,
                            TimeOut = TimeOut,
                            WaitForPort = 0,
                            Ntlm = NtmlAuthenticationRequired,
                            Nodirlist = NoDirList
                        };
            return args;
        }
        private void Start()
        {
            // use CommandLineArguments as a pre validation tool

            CommandLineArguments args = GetArgs();
            ClearError();

            try
            {
                args.Validate();
            }

            catch (CassiniException ex)
            {
                SetError(ex.Field, ex.Message);
                return;
            }

            IPAddress = args.IPAddress;

            Port = args.Port;

            HostName = args.HostName;

            _server = new Server(args.Port, args.VirtualPath, args.ApplicationPath,
                                 System.Net.IPAddress.Parse(args.IPAddress), args.HostName, args.TimeOut, args.Ntlm,
                                 args.Nodirlist);

            if (args.AddHost)
            {
                HostsFile.AddHostEntry(_server.IPAddress.ToString(), _server.HostName);
            }

            try
            {
                _server.Start();
                _server.TimedOut += OnTimedOut;
                UpdateUIFromServer();
                InvokeSetRunState(RunState.Running);
             
            }

            catch (Exception ex)
            {
                SetError(ErrorField.None, ex.Message);
                _server.Dispose();
            }
        }

        /// <summary>
        /// The server could be stopped either by user action,
        /// timeout or exception.  If by timeout, the call will be
        /// coming from another thread in another appdomain far far 
        /// away, so we execise caution and wrap the method that
        /// actual does the stoppage in this invokable wrapper.
        /// </summary>
        private void InvokeStop()
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(Stop));
            }
            else
            {
                Stop();
            }
        }

        private void Stop()
        {
            // kill the start button so we don't get a start 
            // signal before completely stopped.
            ButtonStart.Enabled = false;

            // revert the host file modification, if necessary
            if (AddHost && RunState == RunState.Running)
            {
                HostsFile.RemoveHostEntry(_server.IPAddress.ToString(), _server.HostName);
            }

            if (_server != null)
            {
                _server.TimedOut -= OnTimedOut;
                _server.Dispose();
            }

            RootUrl = string.Empty;

            InvokeSetRunState(RunState.Idle);

            if (_automated)
            {
                ExitApp();
            }
        }



        private static void ExitApp()
        {
            Application.Exit();
        }


        private static void ShowHelp()
        {
            MessageBox.Show("help/about TODO");
        }

        private void ShowLog()
        {
            if (_logForm == null || _logForm.IsDisposed)
            {
                _logForm = new LogView(_server);
            }
            _logForm.Show();
            _logForm.BringToFront();
        }


        private void BrowsePath()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                ApplicationPath = fbd.SelectedPath;
            }
        }

        private void LaunchBrowser()
        {
            Process.Start(RootUrlLinkLabel.Text);
        }


        private IPMode GetIpMode()
        {
            IPMode result;
            if (IPModeAnyRadioButton.Checked)
            {
                result = IPMode.Any;
            }
            else if (IPModeLoopBackRadioButton.Checked)
            {
                result = IPMode.Loopback;
            }
            else
            {
                result = IPMode.Specific;
            }
            return result;
        }

        private PortMode GetPortMode()
        {
            return PortModeSpecificRadioButton.Checked ? PortMode.Specific : PortMode.FirstAvailable;
        }



        private void SetIpMode(IPMode value)
        {
            switch (value)
            {
                case IPMode.Loopback:
                    IPModeLoopBackRadioButton.Checked = true;
                    break;
                case IPMode.Any:
                    IPModeAnyRadioButton.Checked = true;
                    break;
                case IPMode.Specific:
                    RadioButtonIPSpecific.Checked = true;
                    break;
            }
        }

        private void SetPortMode(PortMode value)
        {
            switch (value)
            {
                case PortMode.FirstAvailable:
                    PortModeFirstAvailableRadioButton.Checked = true;
                    break;
                case PortMode.Specific:
                    PortModeSpecificRadioButton.Checked = true;
                    break;
            }
        }



        private void SetError(ErrorField field, string value)
        {
            EnableForm();
            switch (field)
            {
                case ErrorField.ApplicationPath:
                    errorProvider1.SetError(ApplicationPathTextBox, value);
                    break;
                case ErrorField.VirtualPath:
                    errorProvider1.SetError(VirtualPathTextBox, value);
                    break;
                case ErrorField.HostName:
                    errorProvider1.SetError(HostNameTextBox, value);
                    break;
                case ErrorField.IsAddHost:
                    errorProvider1.SetError(AddHostEntryCheckBox, value);
                    break;
                case ErrorField.IPAddress:
                    errorProvider1.SetError(IPSpecificTextBox, value);
                    break;
                case ErrorField.IPAddressAny:
                    errorProvider1.SetError(IPModeAnyRadioButton, value);
                    break;
                case ErrorField.IPAddressLoopBack:
                    errorProvider1.SetError(IPModeLoopBackRadioButton, value);
                    break;
                case ErrorField.Port:
                    errorProvider1.SetError(PortTextBox, value);
                    break;
                case ErrorField.PortRange:
                    errorProvider1.SetError(PortRangeStartTextBox, value);
                    errorProvider1.SetError(PortRangeEndTextBox, value);
                    break;
                case ErrorField.PortRangeStart:
                    errorProvider1.SetError(PortRangeStartTextBox, value);
                    break;
                case ErrorField.PortRangeEnd:
                    errorProvider1.SetError(PortRangeEndTextBox, value);
                    break;
                case ErrorField.None:
                    MessageBox.Show(value, "Error");
                    break;
            }
        }

        private void ClearError()
        {
            errorProvider1.Clear();
        }


        #endregion

        #region Handlers

        private void BrowsePath(object sender, EventArgs e)
        {
            BrowsePath();
        }

        private void HideMainForm(object sender, EventArgs e)
        {
            Close();
        }

        private void LaunchBrowser(object sender, EventArgs e)
        {
            LaunchBrowser();
        }

        private void LaunchBrowser(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LaunchBrowser();
        }

        private void ShowHelp(object sender, EventArgs e)
        {
            ShowHelp();
        }

        private void ShowLog(object sender, EventArgs e)
        {
            ShowLog();
        }

        private void ShowMainForm(object sender, EventArgs e)
        {
            ShowMainForm();
        }

        /// <summary>
        /// Responds to the Start/Stop button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartStop(object sender, EventArgs e)
        {
            StartStop();
        }


        private void ExitApp(object sender, EventArgs e)
        {
            ExitApp();
        }

        private void HostNameChanged(object sender, EventArgs e)
        {
            HostNameChanged();
        }


        /// <summary>
        /// If a timeout value is specifically set and we
        /// get a timeout event, just exit the application.
        /// This should always be the case, but will be 
        /// a bit forgiving here and perform a check before
        /// dumping.
        /// </summary>
        private void OnTimedOut(object sender, EventArgs e)
        {
            InvokeStop();

            if (TimeOut > 0)
            {
                ExitApp();
            }
            else
            {
                ShowMainForm();
            }
        }

        #region Hinky lookin radios that depend on known state. Could be a source of trouble.

        private void RadioButtonIPAny_CheckedChanged(object sender, EventArgs e)
        {
            IPSpecificTextBox.Enabled = false;
            IPV6CheckBox.Enabled = true;
        }

        private void RadioButtonIPLoopBack_CheckedChanged(object sender, EventArgs e)
        {
            IPSpecificTextBox.Enabled = false;
            IPV6CheckBox.Enabled = true;
        }

        private void RadioButtonIPSpecific_CheckedChanged(object sender, EventArgs e)
        {
            IPSpecificTextBox.Enabled = true;
            IPV6CheckBox.Enabled = false;
            IPV6CheckBox.Checked = false;
        }

        private void RadioButtonPortFind_CheckedChanged(object sender, EventArgs e)
        {
            PortTextBox.Enabled = false;
            PortRangeEndTextBox.Enabled = true;
            PortRangeStartTextBox.Enabled = true;
        }

        private void RadioButtonPortSpecific_CheckedChanged(object sender, EventArgs e)
        {
            PortTextBox.Enabled = true;
            PortRangeEndTextBox.Enabled = false;
            PortRangeStartTextBox.Enabled = false;
        }
        #endregion

        #endregion
        

    }
}