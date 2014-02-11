using System;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Processes;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Lifecycle.Commands;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using IServiceProvider = NzbDrone.Common.IServiceProvider;

namespace NzbDrone.Core.Lifecycle
{
    public class LifecycleService: IExecute<ShutdownCommand>, IExecute<RestartCommand>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IRuntimeInfo _runtimeInfo;
        private readonly IServiceProvider _serviceProvider;
        private readonly IProcessProvider _processProvider;
        private readonly Logger _logger;


        public LifecycleService(IEventAggregator eventAggregator,
                                IRuntimeInfo runtimeInfo,
                                IServiceProvider serviceProvider,
                                IProcessProvider processProvider,
                                Logger logger)
        {
            _eventAggregator = eventAggregator;
            _runtimeInfo = runtimeInfo;
            _serviceProvider = serviceProvider;
            _processProvider = processProvider;
            _logger = logger;
        }

        public void Execute(ShutdownCommand message)
        {
            _logger.Info("Shutdown requested.");
            _eventAggregator.PublishEvent(new ApplicationShutdownRequested());
            
            if (_runtimeInfo.IsWindowsService)
            {
                _serviceProvider.Stop(ServiceProvider.NZBDRONE_SERVICE_NAME);
            }
        }

        public void Execute(RestartCommand message)
        {
            _logger.Info("Restart requested.");

            if (OsInfo.IsLinux)
            {
                _processProvider.SpawnNewProcess(_runtimeInfo.ExecutingApplication, "--terminateexisting --nobrowser");
            }

            _eventAggregator.PublishEvent(new ApplicationShutdownRequested(true));

            if (_runtimeInfo.IsWindowsService)
            {
                _serviceProvider.Restart(ServiceProvider.NZBDRONE_SERVICE_NAME);
            }

            else
            {
                _processProvider.SpawnNewProcess(_runtimeInfo.ExecutingApplication, "--terminateexisting --nobrowser");
            }
        }
    }
}
