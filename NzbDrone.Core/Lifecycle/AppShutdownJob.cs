using System;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Jobs.Framework;
using NzbDrone.Core.Model.Notification;

namespace NzbDrone.Core.Lifecycle
{
    public class AppShutdownJob : IJob
    {
        private readonly EnvironmentProvider _environmentProvider;
        private readonly ProcessProvider _processProvider;
        private readonly ServiceProvider _serviceProvider;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public AppShutdownJob(EnvironmentProvider environmentProvider, ProcessProvider processProvider, ServiceProvider serviceProvider)
        {
            _environmentProvider = environmentProvider;
            _processProvider = processProvider;
            _serviceProvider = serviceProvider;
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
            logger.Info("Shutting down NzbDrone");

            if (_serviceProvider.ServiceExist(ServiceProvider.NZBDRONE_SERVICE_NAME)
               && _serviceProvider.IsServiceRunning(ServiceProvider.NZBDRONE_SERVICE_NAME))
            {
                logger.Debug("Stopping NzbDrone Service");
                _serviceProvider.Stop(ServiceProvider.NZBDRONE_SERVICE_NAME);
            }

            else
            {
                logger.Debug("Stopping NzbDrone console");

                var pid = _environmentProvider.NzbDroneProcessIdFromEnviroment;
                _processProvider.Kill(pid);
            }
        }
    }
}