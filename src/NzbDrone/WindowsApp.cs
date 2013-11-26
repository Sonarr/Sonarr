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
        private static readonly Logger Logger = NzbDroneLogger.GetLogger();

        public static void Main(string[] args)
        {
            try
            {
                var startupArgs = new StartupArguments(args);

                LogTargets.Register(startupArgs, false, true);

                var bootstrap = new Bootstrap(startupArgs, new MessageBoxUserAlert());

                bootstrap.EnsureSingleInstance();

                bootstrap.Start();
                bootstrap.Container.Register<ISystemTrayApp, SystemTrayApp>();
                bootstrap.Container.Resolve<ISystemTrayApp>().Start();

            }
            catch (TerminateApplicationException e)
            {
                Logger.Info("Application has been terminated. Reason " + e.Reason);
            }
            catch (Exception e)
            {
                Logger.FatalException("EPIC FAIL: " + e.Message, e);
                var message = string.Format("{0}: {1}", e.GetType().Name, e.Message);
                MessageBox.Show(text: message, buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error, caption: "Epic Fail!");
            }
        }


    }
}
