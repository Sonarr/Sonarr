using System;
using System.Collections.Specialized;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using NLog;

namespace NzbDrone.Common
{
    public interface IServiceProvider
    {
        bool ServiceExist(string name);
        bool IsServiceRunning(string name);
        void Install(string serviceName);
        void UnInstall(string serviceName);
        void Run(ServiceBase service);
        ServiceController GetService(string serviceName);
        void Stop(string serviceName);
        void Start(string serviceName);
    }

    public class ServiceProvider : IServiceProvider
    {
        public const string NZBDRONE_SERVICE_NAME = "NzbDrone";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public virtual bool ServiceExist(string name)
        {
            Logger.Debug("Checking if service {0} exists.", name);
            return
                ServiceController.GetServices().Any(
                    s => String.Equals(s.ServiceName, name, StringComparison.InvariantCultureIgnoreCase));
        }

        public virtual bool IsServiceRunning(string name)
        {
            Logger.Debug("Checking if '{0}' service is running", name);

            var service = ServiceController.GetServices()
                .SingleOrDefault(s => String.Equals(s.ServiceName, name, StringComparison.InvariantCultureIgnoreCase));

            return service != null && (
                service.Status != ServiceControllerStatus.Stopped || 
                service.Status == ServiceControllerStatus.StopPending || 
                service.Status == ServiceControllerStatus.Paused || 
                service.Status == ServiceControllerStatus.PausePending);
        }

        public virtual void Install(string serviceName)
        {
            Logger.Info("Installing service '{0}'", serviceName);


            var installer = new ServiceProcessInstaller
                                {
                                    Account = ServiceAccount.LocalSystem
                                };

            var serviceInstaller = new ServiceInstaller();


            String[] cmdline = { @"/assemblypath=" + Process.GetCurrentProcess().MainModule.FileName };

            var context = new InstallContext("service_install.log", cmdline);
            serviceInstaller.Context = context;
            serviceInstaller.DisplayName = serviceName;
            serviceInstaller.ServiceName = serviceName;
            serviceInstaller.Description = "NzbDrone Application Server";
            serviceInstaller.StartType = ServiceStartMode.Automatic;

            serviceInstaller.Parent = installer;

            serviceInstaller.Install(new ListDictionary());

            Logger.Info("Service Has installed successfully.");
        }

        public virtual void UnInstall(string serviceName)
        {
            Logger.Info("Uninstalling {0} service", serviceName);

            Stop(serviceName);

            var serviceInstaller = new ServiceInstaller();

            var context = new InstallContext("service_uninstall.log", null);
            serviceInstaller.Context = context;
            serviceInstaller.ServiceName = serviceName;
            serviceInstaller.Uninstall(null);

            Logger.Info("{0} successfully uninstalled", serviceName);
        }

        public virtual void Run(ServiceBase service)
        {
            ServiceBase.Run(service);
        }

        public virtual ServiceController GetService(string serviceName)
        {
            return ServiceController.GetServices().FirstOrDefault(c => String.Equals(c.ServiceName, serviceName, StringComparison.InvariantCultureIgnoreCase));
        }

        public virtual void Stop(string serviceName)
        {
            Logger.Info("Stopping {0} Service...", serviceName);
            var service = GetService(serviceName);
            if (service == null)
            {
                Logger.Warn("Unable to stop {0}. no service with that name exists.", serviceName);
                return;
            }

            Logger.Info("Service is currently {0}", service.Status);

            if (service.Status != ServiceControllerStatus.Stopped)
            {
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(60));

                service.Refresh();
                if (service.Status == ServiceControllerStatus.Stopped)
                {
                    Logger.Info("{0} has stopped successfully.", serviceName);
                }
                else
                {
                    Logger.Error("Service stop request has timed out. {0}", service.Status);
                }
            }
            else
            {
                Logger.Warn("Service {0} is already in stopped state.", service.ServiceName);
            }
        }

        public virtual void Start(string serviceName)
        {
            Logger.Info("Starting {0} Service...", serviceName);
            var service = GetService(serviceName);
            if (service == null)
            {
                Logger.Warn("Unable to start '{0}' no service with that name exists.", serviceName);
                return;
            }

            if (service.Status != ServiceControllerStatus.Paused && service.Status != ServiceControllerStatus.Stopped)
            {
                Logger.Warn("Service is in a state that can't be started. Current status: {0}", service.Status);
            }

            service.Start();

            service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(60));
            service.Refresh();

            if (service.Status == ServiceControllerStatus.Running)
            {
                Logger.Info("{0} has started successfully.", serviceName);
            }
            else
            {
                Logger.Error("Service start request has timed out. {0}", service.Status);
            }
        }
    }
}