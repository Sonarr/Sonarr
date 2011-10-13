using System;
using System.Collections.Specialized;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using NLog;

namespace NzbDrone.Providers
{
    public class ServiceProvider
    {
        public const string NzbDroneServiceName = "NzbDrone";

        private static readonly Logger Logger = LogManager.GetLogger("Host.ServiceManager");


        public bool ServiceExist(string name)
        {
            return
                ServiceController.GetServices().Any(
                    s => String.Equals(s.ServiceName, name, StringComparison.InvariantCultureIgnoreCase));
        }


        public virtual void Install()
        {
            Logger.Info("Installing service '{0}'", NzbDroneServiceName);


            var installer = new ServiceProcessInstaller
                                {
                                    Account = ServiceAccount.NetworkService
                                };

            var serviceInstaller = new ServiceInstaller();


            String[] cmdline = {@"/assemblypath=" + Assembly.GetExecutingAssembly().Location};

            var context = new InstallContext("service_install.log", cmdline);
            serviceInstaller.Context = context;
            serviceInstaller.DisplayName = NzbDroneServiceName;
            serviceInstaller.ServiceName = NzbDroneServiceName;
            serviceInstaller.StartType = ServiceStartMode.Automatic;
            
            
            serviceInstaller.Parent = installer;

            serviceInstaller.Install(new ListDictionary());

            Logger.Info("Service Has installed successfully.");
        }

        public virtual void UnInstall()
        {
            var serviceInstaller = new ServiceInstaller();

            var context = new InstallContext("service_uninstall.log", null);
            serviceInstaller.Context = context;
            serviceInstaller.ServiceName = NzbDroneServiceName;
            serviceInstaller.Uninstall(null);
        }
    }
}