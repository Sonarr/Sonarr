using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Processes;
using NzbDrone.Core.Lifecycle.Commands;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using IServiceProvider = NzbDrone.Common.IServiceProvider;

namespace NzbDrone.Core.Lifecycle
{
    public class LifestyleService: IExecute<ShutdownCommand>, IExecute<RestartCommand>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IRuntimeInfo _runtimeInfo;
        private readonly IServiceProvider _serviceProvider;
        private readonly IProcessProvider _processProvider;


        public LifestyleService(IEventAggregator eventAggregator,
                                IRuntimeInfo runtimeInfo,
                                IServiceProvider serviceProvider,
                                IProcessProvider processProvider)
        {
            _eventAggregator = eventAggregator;
            _runtimeInfo = runtimeInfo;
            _serviceProvider = serviceProvider;
            _processProvider = processProvider;
        }

        public void Execute(ShutdownCommand message)
        {
            if (_runtimeInfo.IsWindowsService)
            {
                _serviceProvider.Stop(ServiceProvider.NZBDRONE_SERVICE_NAME);
            }

            else
            {
                _eventAggregator.PublishEvent(new ApplicationShutdownRequested());
            }
        }

        public void Execute(RestartCommand message)
        {
            if (_runtimeInfo.IsWindowsService)
            {
                _serviceProvider.Restart(ServiceProvider.NZBDRONE_SERVICE_NAME);
            }

            _eventAggregator.PublishEvent(new ApplicationRestartRequested());
        }
    }
}
