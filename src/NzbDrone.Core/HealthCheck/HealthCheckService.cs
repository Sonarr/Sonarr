using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Configuration.Events;
using NzbDrone.Core.Download;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider.Events;

namespace NzbDrone.Core.HealthCheck
{
    public interface IHealthCheckService
    {
        List<HealthCheck> PerformHealthCheck();
    }

    public class HealthCheckService : IHealthCheckService,
                                      IExecute<CheckHealthCommand>,
                                      IHandleAsync<ConfigSavedEvent>,
                                      IHandleAsync<ProviderUpdatedEvent<IIndexer>>,
                                      IHandleAsync<ProviderUpdatedEvent<IDownloadClient>>
    {
        private readonly IEnumerable<IProvideHealthCheck> _healthChecks;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public HealthCheckService(IEnumerable<IProvideHealthCheck> healthChecks, IEventAggregator eventAggregator, Logger logger)
        {
            _healthChecks = healthChecks;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public List<HealthCheck> PerformHealthCheck()
        {
            _logger.Trace("Checking health");
            var result = _healthChecks.Select(c => c.Check()).Where(c => c != null).ToList();
            
            return result;
        }

        public void Execute(CheckHealthCommand message)
        {
            //Until we have stored health checks we should just trigger the complete event
            //and let the clients check in
            //Multiple connected clients means we're going to compute the health check multiple times
            //Multiple checks feels a bit ugly, but means the most up to date information goes to the client
            _eventAggregator.PublishEvent(new TriggerHealthCheckEvent());
        }

        public void HandleAsync(ConfigSavedEvent message)
        {
            _eventAggregator.PublishEvent(new TriggerHealthCheckEvent());
        }

        public void HandleAsync(ProviderUpdatedEvent<IIndexer> message)
        {
            _eventAggregator.PublishEvent(new TriggerHealthCheckEvent());
        }

        public void HandleAsync(ProviderUpdatedEvent<IDownloadClient> message)
        {
            _eventAggregator.PublishEvent(new TriggerHealthCheckEvent());
        }
    }
}
