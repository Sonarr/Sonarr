using NLog;
using Workarr.Common;
using Workarr.EnvironmentInfo;
using Workarr.Lifecycle.Commands;
using Workarr.Messaging.Commands;
using Workarr.Messaging.Events;
using IServiceProvider = Workarr.Common.IServiceProvider;

namespace Workarr.Lifecycle
{
    public interface ILifecycleService
    {
        void Shutdown();
        void Restart();
    }

    public class LifecycleService : ILifecycleService, IExecute<ShutdownCommand>, IExecute<RestartCommand>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IRuntimeInfo _runtimeInfo;
        private readonly IServiceProvider _serviceProvider;
        private readonly Logger _logger;

        public LifecycleService(IEventAggregator eventAggregator,
                                IRuntimeInfo runtimeInfo,
                                IServiceProvider serviceProvider,
                                Logger logger)
        {
            _eventAggregator = eventAggregator;
            _runtimeInfo = runtimeInfo;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public void Shutdown()
        {
            _logger.Info("Shutdown requested.");
            _eventAggregator.PublishEvent(new ApplicationShutdownRequested());

            if (_runtimeInfo.IsWindowsService)
            {
                _serviceProvider.Stop(ServiceProvider.SERVICE_NAME);
            }
        }

        public void Restart()
        {
            _logger.Info("Restart requested.");

            _eventAggregator.PublishEvent(new ApplicationShutdownRequested(true));

            if (_runtimeInfo.IsWindowsService)
            {
                _serviceProvider.Restart(ServiceProvider.SERVICE_NAME);
            }
        }

        public void Execute(ShutdownCommand message)
        {
            Shutdown();
        }

        public void Execute(RestartCommand message)
        {
            Restart();
        }
    }
}
