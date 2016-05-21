using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Core.Configuration.Events;
using NzbDrone.Core.Download;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider.Events;

namespace NzbDrone.Core.HealthCheck
{
    public interface IHealthCheckService
    {
        List<HealthCheck> Results();
    }

    public class HealthCheckService : IHealthCheckService,
                                      IExecute<CheckHealthCommand>,
                                      IHandleAsync<ApplicationStartedEvent>,
                                      IHandleAsync<ConfigSavedEvent>,
                                      IHandleAsync<ProviderUpdatedEvent<IIndexer>>,
                                      IHandleAsync<ProviderDeletedEvent<IIndexer>>,
                                      IHandleAsync<ProviderUpdatedEvent<IDownloadClient>>,
                                      IHandleAsync<ProviderDeletedEvent<IDownloadClient>>
    {
        private readonly IEnumerable<IProvideHealthCheck> _healthChecks;
        private readonly IEventAggregator _eventAggregator;
        private readonly ICacheManager _cacheManager;
        private readonly Logger _logger;

        private readonly ICached<HealthCheck> _healthCheckResults;

        public HealthCheckService(IEnumerable<IProvideHealthCheck> healthChecks,
                                  IEventAggregator eventAggregator,
                                  ICacheManager cacheManager,
                                  Logger logger)
        {
            _healthChecks = healthChecks;
            _eventAggregator = eventAggregator;
            _cacheManager = cacheManager;
            _logger = logger;

            _healthCheckResults = _cacheManager.GetCache<HealthCheck>(GetType());
        }

        public List<HealthCheck> Results()
        {
            return _healthCheckResults.Values.ToList();
        }

        private void PerformHealthCheck(Func<IProvideHealthCheck, bool> predicate)
        {
            var results = _healthChecks.Where(predicate)
                                       .Select(c => c.Check())
                                       .ToList();

            foreach (var result in results)
            {
                if (result.Type == HealthCheckResult.Ok)
                {
                    _healthCheckResults.Remove(result.Source.Name);
                }

                else
                {
                    _healthCheckResults.Set(result.Source.Name, result);
                }
            }

            _eventAggregator.PublishEvent(new HealthCheckCompleteEvent());
        }

        public void Execute(CheckHealthCommand message)
        {
            PerformHealthCheck(c => message.Trigger == CommandTrigger.Manual || c.CheckOnSchedule);
        }

        public void HandleAsync(ApplicationStartedEvent message)
        {
            PerformHealthCheck(c => c.CheckOnStartup);
        }

        public void HandleAsync(ConfigSavedEvent message)
        {
            PerformHealthCheck(c => c.CheckOnConfigChange);
        }

        public void HandleAsync(ProviderUpdatedEvent<IIndexer> message)
        {
            PerformHealthCheck(c => c.CheckOnConfigChange);
        }

        public void HandleAsync(ProviderDeletedEvent<IIndexer> message)
        {
            PerformHealthCheck(c => c.CheckOnConfigChange);
        }

        public void HandleAsync(ProviderUpdatedEvent<IDownloadClient> message)
        {
            PerformHealthCheck(c => c.CheckOnConfigChange);
        }

        public void HandleAsync(ProviderDeletedEvent<IDownloadClient> message)
        {
            PerformHealthCheck(c => c.CheckOnConfigChange);
        }
    }
}
