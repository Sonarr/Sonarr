using System;
using System.Windows.Forms;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Host;
using NzbDrone.SysTray;

namespace NzbDrone
{
    public static class WindowsApp
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
                Logger.FatalException(e.Message, e);
                var message = string.Format("{0}: {1}", e.GetType().Name, e.Message);
                MessageBox.Show(text: message, buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error, caption: "Epic Fail!");
            }
        }
    }
}