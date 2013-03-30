using System;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Model.Notification;

namespace NzbDrone.Core.Lifecycle
{
    public class AppShutdownJob : IJob
    {
        private readonly ProcessProvider _processProvider;
        private readonly ServiceProvider _serviceProvider;
        private readonly Logger _logger;


        public AppShutdownJob(ProcessProvider processProvider, ServiceProvider serviceProvider, Logger logger)
        {
            _processProvider = processProvider;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public string Name
        {
            get { return "Shutdown NzbDrone"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromTicks(0); }
        }

        public virtual void Start(ProgressNotification notification, dynamic options)
        {
            notification.CurrentMessage = "Shutting down NzbDrone";
            _logger.Info("Shutting down NzbDrone");

            if (_serviceProvider.ServiceExist(ServiceProvider.NZBDRONE_SERVICE_NAME)
               && _serviceProvider.IsServiceRunning(ServiceProvider.NZBDRONE_SERVICE_NAME))
            {
                _logger.Debug("Stopping NzbDrone Service");
                _serviceProvider.Stop(ServiceProvider.NZBDRONE_SERVICE_NAME);
            }

            else
            {
                _logger.Debug("Stopping NzbDrone console");

                var currentProcess = _processProvider.GetCurrentProcess();
                _processProvider.Kill(currentProcess.Id);
            }
        }
    }
}