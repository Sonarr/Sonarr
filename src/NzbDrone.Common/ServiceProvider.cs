using System;
using System.Collections.Specialized;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Processes;

namespace NzbDrone.Common
{
    public interface IServiceProvider
    {
        bool ServiceExist(string name);
        bool IsServiceRunning(string name);
        void Install(string serviceName);
        void Uninstall(string serviceName);
        void Run(ServiceBase service);
        ServiceController GetService(string serviceName);
        void Stop(string serviceName);
        void Start(string serviceName);
        ServiceControllerStatus GetStatus(string serviceName);
        void Restart(string serviceName);
        void SetPermissions(string serviceName);
    }

    public class ServiceProvider : IServiceProvider
    {
        public const string SERVICE_NAME = "Sonarr";

        private readonly IProcessProvider _processProvider;
        private readonly Logger _logger;

        public ServiceProvider(IProcessProvider processProvider, Logger logger)
        {
            _processProvider = processProvider;
            _logger = logger;
        }

        public virtual bool ServiceExist(string name)
        {
            _logger.Debug("Checking if service {0} exists.", name);
            return
                ServiceController.GetServices().Any(
                    s => string.Equals(s.ServiceName, name, StringComparison.InvariantCultureIgnoreCase));
        }

        public virtual bool IsServiceRunning(string name)
        {
            _logger.Debug("Checking if '{0}' service is running", name);

            var service = ServiceController.GetServices()
                .SingleOrDefault(s => string.Equals(s.ServiceName, name, StringComparison.InvariantCultureIgnoreCase));

            return service != null && (
                service.Status != ServiceControllerStatus.Stopped ||
                service.Status == ServiceControllerStatus.StopPending ||
                service.Status == ServiceControllerStatus.Paused ||
                service.Status == ServiceControllerStatus.PausePending);
        }

        public virtual void Install(string serviceName)
        {
            _logger.Info("Installing service '{0}'", serviceName);


            var installer = new ServiceProcessInstaller
                                {
                                    Account = ServiceAccount.LocalService
                                };

            var serviceInstaller = new ServiceInstaller();


            string[] cmdline = { @"/assemblypath=" + Process.GetCurrentProcess().MainModule.FileName };

            var context = new InstallContext("service_install.log", cmdline);
            serviceInstaller.Context = context;
            serviceInstaller.DisplayName = serviceName;
            serviceInstaller.ServiceName = serviceName;
            serviceInstaller.Description = "Sonarr Application Server";
            serviceInstaller.StartType = ServiceStartMode.Automatic;
            serviceInstaller.ServicesDependedOn = new[] { "EventLog", "Tcpip", "http" };

            serviceInstaller.Parent = installer;

            serviceInstaller.Install(new ListDictionary());

            _logger.Info("Service Has installed successfully.");
        }

        public virtual void Uninstall(string serviceName)
        {
            _logger.Info("Uninstalling {0} service", serviceName);

            Stop(serviceName);

            var serviceInstaller = new ServiceInstaller();

            var context = new InstallContext("service_uninstall.log", null);
            serviceInstaller.Context = context;
            serviceInstaller.ServiceName = serviceName;
            serviceInstaller.Uninstall(null);

            _logger.Info("{0} successfully uninstalled", serviceName);
        }

        public virtual void Run(ServiceBase service)
        {
            ServiceBase.Run(service);
        }

        public virtual ServiceController GetService(string serviceName)
        {
            return ServiceController.GetServices().FirstOrDefault(c => string.Equals(c.ServiceName, serviceName, StringComparison.InvariantCultureIgnoreCase));
        }

        public virtual void Stop(string serviceName)
        {
            _logger.Info("Stopping {0} Service...", serviceName);
            var service = GetService(serviceName);
            if (service == null)
            {
                _logger.Warn("Unable to stop {0}. no service with that name exists.", serviceName);
                return;
            }

            _logger.Info("Service is currently {0}", service.Status);

            if (service.Status != ServiceControllerStatus.Stopped)
            {
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(60));

                service.Refresh();
                if (service.Status == ServiceControllerStatus.Stopped)
                {
                    _logger.Info("{0} has stopped successfully.", serviceName);
                }
                else
                {
                    _logger.Error("Service stop request has timed out. {0}", service.Status);
                }
            }
            else
            {
                _logger.Warn("Service {0} is already in stopped state.", service.ServiceName);
            }
        }

        public ServiceControllerStatus GetStatus(string serviceName)
        {
          return  GetService(serviceName).Status;
        }

        public void Start(string serviceName)
        {
            _logger.Info("Starting {0} Service...", serviceName);
            var service = GetService(serviceName);
            if (service == null)
            {
                _logger.Warn("Unable to start '{0}' no service with that name exists.", serviceName);
                return;
            }

            if (service.Status != ServiceControllerStatus.Paused && service.Status != ServiceControllerStatus.Stopped)
            {
                _logger.Warn("Service is in a state that can't be started. Current status: {0}", service.Status);
            }

            service.Start();

            service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(60));
            service.Refresh();

            if (service.Status == ServiceControllerStatus.Running)
            {
                _logger.Info("{0} has started successfully.", serviceName);
            }
            else
            {
                _logger.Error("Service start request has timed out. {0}", service.Status);
            }
        }

        public void Restart(string serviceName)
        {
            var args = string.Format("/C net.exe stop \"{0}\" && net.exe start \"{0}\"", serviceName);

            _processProvider.Start("cmd.exe", args);
        }

        public void SetPermissions(string serviceName)
        {
            var dacls = GetServiceDacls(serviceName);
            SetServiceDacls(serviceName, dacls);
        }

        private string GetServiceDacls(string serviceName)
        {
            var output = _processProvider.StartAndCapture("sc.exe", $"sdshow {serviceName}");

            var dacls = output.Standard.Select(s => s.Content).Where(s => s.IsNotNullOrWhiteSpace()).ToList();

            if (dacls.Count == 1)
            {
                return dacls[0];
            }

            throw new ArgumentException("Invalid DACL output");
        }

        private void SetServiceDacls(string serviceName, string dacls)
        {
            const string authenticatedUsersDacl = "(A;;CCLCSWRPWPLOCRRC;;;AU)";

            if (dacls.Contains(authenticatedUsersDacl))
            {
                // Permssions already set
                return;
            }

            var indexOfS = dacls.IndexOf("S:", StringComparison.InvariantCultureIgnoreCase);

            dacls = indexOfS == -1 ? $"{dacls}{authenticatedUsersDacl}" : dacls.Insert(indexOfS, authenticatedUsersDacl);

            _processProvider.Start("sc.exe", $"sdset {serviceName} {dacls}").WaitForExit();
        }
    }
}
