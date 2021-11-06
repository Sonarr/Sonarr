using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Hosting;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Processes;
using NzbDrone.Host;

namespace NzbDrone.SysTray
{
    public class SystemTrayApp : Form, IHostedService
    {
        private readonly IBrowserService _browserService;
        private readonly IRuntimeInfo _runtimeInfo;
        private readonly IProcessProvider _processProvider;

        private readonly NotifyIcon _trayIcon = new NotifyIcon();
        private readonly ContextMenuStrip _trayMenu = new ContextMenuStrip();

        public SystemTrayApp(IBrowserService browserService, IRuntimeInfo runtimeInfo, IProcessProvider processProvider)
        {
            _browserService = browserService;
            _runtimeInfo = runtimeInfo;
            _processProvider = processProvider;
        }

        public void Start()
        {
            Application.ThreadException += OnThreadException;
            Application.ApplicationExit += OnApplicationExit;

            Application.SetHighDpiMode(HighDpiMode.PerMonitor);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            _trayMenu.Items.Add(new ToolStripMenuItem("Launch Browser", null, LaunchBrowser));
            _trayMenu.Items.Add(new ToolStripSeparator());
            _trayMenu.Items.Add(new ToolStripMenuItem("Exit", null, OnExit));

            _trayIcon.Text = string.Format("Sonarr - {0}", BuildInfo.Version);
            _trayIcon.Icon = Properties.Resources.Sonarr;

            _trayIcon.ContextMenuStrip = _trayMenu;
            _trayIcon.Visible = true;
            _trayIcon.DoubleClick += LaunchBrowser;

            Application.Run(this);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var thread = new Thread(Start);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            DisposeTrayIcon();
        }

        protected override void OnClosed(EventArgs e)
        {
            Console.WriteLine("Closing");
            base.OnClosed(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;

            base.OnLoad(e);
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                _trayIcon.Dispose();
            }

            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => Dispose(isDisposing)));
            }
            else
            {
                base.Dispose(isDisposing);
            }
        }

        private void OnExit(object sender, EventArgs e)
        {
            LogManager.Configuration = null;
            Environment.Exit(0);
        }

        private void LaunchBrowser(object sender, EventArgs e)
        {
            try
            {
                _browserService.LaunchWebUI();
            }
            catch (Exception)
            {
            }
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            if (_runtimeInfo.RestartPending)
            {
                _processProvider.SpawnNewProcess(_runtimeInfo.ExecutingApplication, "--restart --nobrowser");
            }

            DisposeTrayIcon();
        }

        private void OnThreadException(object sender, EventArgs e)
        {
            DisposeTrayIcon();
        }

        private void DisposeTrayIcon()
        {
            try
            {
                _trayIcon.Visible = false;
                _trayIcon.Icon = null;
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
            }
            catch (Exception)
            {
            }
        }
    }
}
