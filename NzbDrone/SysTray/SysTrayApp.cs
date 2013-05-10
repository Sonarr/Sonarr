using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using NzbDrone.Common;
using NzbDrone.Owin;

namespace NzbDrone.SysTray
{
    public interface ISystemTrayApp
    {
        void Start();
    }

    public class SystemTrayApp : Form, ISystemTrayApp
    {
        private readonly IProcessProvider _processProvider;
        private readonly IHostController _hostController;
        private readonly IEnvironmentProvider _environmentProvider;

        private readonly NotifyIcon _trayIcon = new NotifyIcon();
        private readonly ContextMenu _trayMenu = new ContextMenu();

        public SystemTrayApp(IProcessProvider processProvider, IHostController hostController, IEnvironmentProvider environmentProvider)
        {
            _processProvider = processProvider;
            _hostController = hostController;
            _environmentProvider = environmentProvider;
        }


        public void Start()
        {
            _trayMenu.MenuItems.Add("Launch Browser", LaunchBrowser);
            _trayMenu.MenuItems.Add("-");
            _trayMenu.MenuItems.Add("Exit", OnExit);

            _trayIcon.Text = String.Format("NzbDrone - {0}", _environmentProvider.Version);
            _trayIcon.Icon = new Icon(Assembly.GetEntryAssembly().GetManifestResourceStream("NzbDrone.NzbDrone.ico"));

            _trayIcon.ContextMenu = _trayMenu;
            _trayIcon.Visible = true;


            Application.Run(this);
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