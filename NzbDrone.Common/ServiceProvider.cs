using System;
using System.Collections.Specialized;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using NLog;
using TimeoutException = System.TimeoutException;

namespace NzbDrone.Common
{
    public class ServiceProvider
    {
        public const string NzbDroneServiceName = "NzbDrone";

        private static readonly Logger Logger = LogManager.GetLogger("Host.ServiceManager");

        public virtual bool ServiceExist(string name)
        {
            Logger.Debug("Checking if service {0} exists.", name);
            return
                ServiceController.GetServices().Any(
                    s => String.Equals(s.ServiceName, name, StringComparison.InvariantCultureIgnoreCase));
        }


        public virtual void Install()
        {
            Logger.Info("Installing service '{0}'", NzbDroneServiceName);


            var installer = new ServiceProcessInstaller
                                {
                                    Account = ServiceAccount.LocalSystem
                                };

            var serviceInstaller = new ServiceInstaller();


            String[] cmdline = { @"/assemblypath=" + Assembly.GetExecutingAssembly().Location };

            var context = new InstallContext("service_install.log", cmdline);
            serviceInstaller.Context = context;
            serviceInstaller.DisplayName = NzbDroneServiceName;
            serviceInstaller.ServiceName = NzbDroneServiceName;
            serviceInstaller.Description = "NzbDrone Application Server";
            serviceInstaller.StartType = ServiceStartMode.Automatic;

            serviceInstaller.Parent = installer;

            serviceInstaller.Install(new ListDictionary());

            Logger.Info("Service Has installed successfully.");
        }

        public virtual void UnInstall()
        {
            Logger.Info("Uninstalling NzbDrone service");
            var serviceInstaller = new ServiceInstaller();

            var context = new InstallContext("service_uninstall.log", null);
            serviceInstaller.Context = context;
            serviceInstaller.ServiceName = NzbDroneServiceName;
            serviceInstaller.Uninstall(null);

            Logger.Info("NzbDrone successfully uninstalled");
        }


        public virtual void Run(ServiceBase service)
        {
            ServiceBase.Run(service);
        }

        public virtual ServiceController GetService(string serviceName)
        {
            return ServiceController.GetServices().Where(
                    c => String.Equals(c.ServiceName, serviceName, StringComparison.InvariantCultureIgnoreCase))
                    .FirstOrDefault();
        }

        public virtual void Stop(string serviceName)
        {
            Logger.Info("Stopping {0} Service...");
            var service = GetService(serviceName);
            if (service == null)
            {
                Logger.Warn("Unable to stop {0}. no service with that name exists.", serviceName);
            }

            Logger.Info("Service is currently {0}", service.Status);

            service.Stop();
            service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(60));

            service.Refresh();
            if (service.Status == ServiceControllerStatus.Stopped)
            {
                Logger.Info("{0} has stopped successfully.");
            }
            else
            {
                Logger.Error("Service stop request has timed out. {0}", service.Status);
            }
        }

        public virtual void Start(string serviceName)
        {
            Logger.Info("Starting {0} Service...");
            var service = GetService(serviceName);
            if (service == null)
            {
                Logger.Warn("Unable to start '{0}' no service with that name exists.", serviceName);
            }

            if (service.Status != ServiceControllerStatus.Paused || service.Status != ServiceControllerStatus.Stopped)
            {
                Logger.Warn("Service is in a state that can't be started {0}", service.Status);
            }

            service.Start();

            service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(60));
            service.Refresh();

            if (service.Status == ServiceControllerStatus.Running)
            {
                Logger.Info("{0} has started successfully.");
            }
            else
            {
                Logger.Error("Service start request has timed out. {0}", service.Status);
            }
        }
    }
}