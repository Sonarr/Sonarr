using System;
using System.Windows.Forms;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Host;
using NzbDrone.SysTray;

namespace NzbDrone
{
    public static class WindowsApp
    {
        public static void Main(string[] args)
        {
            try
            {
                var container = Bootstrap.Start(new StartupArguments(args), new MessageBoxUserAlert());
                container.Register<ISystemTrayApp, SystemTrayApp>();
                container.Resolve<ISystemTrayApp>().Start();
            }
            catch (TerminateApplicationException)
            {
            }
            catch (Exception e)
            {
                var message = string.Format("{0}: {1}", e.GetType().Name, e.Message);
                MessageBox.Show(text: message, buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error, caption: "Epic Fail!");
            }
        }
    }
}