using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace NzbDrone.Common.SysTray
{
    public class SysTrayApp : Form
    {
        private readonly ProcessProvider _processProvider;
        private readonly IHostController _hostController;
        private readonly EnvironmentProvider _environmentProvider;

        private readonly NotifyIcon _trayIcon = new NotifyIcon();
        private readonly ContextMenu _trayMenu = new ContextMenu();

        public SysTrayApp(ProcessProvider processProvider, IHostController hostController, EnvironmentProvider environmentProvider)
        {
            _processProvider = processProvider;
            _hostController = hostController;
            _environmentProvider = environmentProvider;
        }

        public SysTrayApp()
        {
        }

        public void Create()
        {
            _trayMenu.MenuItems.Add("Launch Browser", LaunchBrowser);
            _trayMenu.MenuItems.Add("-");
            _trayMenu.MenuItems.Add("Exit", OnExit);

            _trayIcon.Text = String.Format("NzbDrone - {0}", _environmentProvider.Version);
            _trayIcon.Icon = new Icon(Assembly.GetEntryAssembly().GetManifestResourceStream("NzbDrone.NzbDrone.ico"));

            _trayIcon.ContextMenu = _trayMenu;
            _trayIcon.Visible = true;
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

            base.Dispose(isDisposing);
        }

        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void LaunchBrowser(object sender, EventArgs e)
        {
            _processProvider.Start(_hostController.AppUrl);
        }
    }
}