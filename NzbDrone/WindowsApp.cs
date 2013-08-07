using System;
using System.Windows.Forms;
using NzbDrone.SysTray;

namespace NzbDrone
{
    public static class WindowsApp
    {
        public static void Main(string[] args)
        {
            try
            {
                var container = Host.Bootstrap.Start(args);
                container.Register<ISystemTrayApp, SystemTrayApp>();
                container.Resolve<ISystemTrayApp>().Start();
            }
            catch (Exception e)
            {
                var message = string.Format("{0}: {1}", e.GetType().Name, e.Message);
                MessageBox.Show(text: message, buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error, caption: "Epic Fail!");
            }
        }
    }
}