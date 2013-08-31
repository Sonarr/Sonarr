using System;
using System.Windows.Forms;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Host;
using NzbDrone.SysTray;

namespace NzbDrone
{
    public static class WindowsApp
    {
        private static readonly Logger Logger =  NzbDroneLogger.GetLogger();

        public static void Main(string[] args)
        {
            try
            {
                var startupArgs = new StartupArguments(args);

                LogTargets.Register(startupArgs, false, true);

                var container = Bootstrap.Start(startupArgs, new MessageBoxUserAlert());
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
