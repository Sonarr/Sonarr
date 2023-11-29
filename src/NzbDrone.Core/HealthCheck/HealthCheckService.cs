using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Messaging;
using NzbDrone.Common.Reflection;
using NzbDrone.Common.TPL;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.HealthCheck
{
    public interface IHealthCheckService
    {
        List<HealthCheck> Results();
    }

    public class HealthCheckService : IHealthCheckService,
                                      IExecute<CheckHealthCommand>,
                                      IHandleAsync<ApplicationStartedEvent>,
                                      IHandleAsync<IEvent>
    {
        private readonly DateTime _startupGracePeriodEndTime;
        private readonly IProvideHealthCheck[] _healthChecks;
        private readonly IProvideHealthCheck[] _startupHealthChecks;
        private readonly IProvideHealthCheck[] _scheduledHealthChecks;
        private readonly Dictionary<Type, IEventDrivenHealthCheck[]> _eventDrivenHealthChecks;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        private readonly ICached<HealthCheck> _healthCheckResults;
        private readonly HashSet<IProvideHealthCheck> _pendingHealthChecks;
        private readonly Debouncer _debounce;

        private bool _hasRunHealthChecksAfterGracePeriod;
        private bool _isRunningHealthChecksAfterGracePeriod;

        public HealthCheckService(IEnumerable<IProvideHealthCheck> healthChecks,
                                  IEventAggregator eventAggregator,
                                  ICacheManager cacheManager,
                                  IDebounceManager debounceManager,
                                  IRuntimeInfo runtimeInfo,
                                  Logger logger)
        {
            _healthChecks = healthChecks.ToArray();
            _eventAggregator = eventAggregator;
            _logger = logger;

            _healthCheckResults = cacheManager.GetCache<HealthCheck>(GetType());
            _pendingHealthChecks = new HashSet<IProvideHealthCheck>();
            _debounce = debounceManager.CreateDebouncer(ProcessHealthChecks, TimeSpan.FromSeconds(5));

            _startupHealthChecks = _healthChecks.Where(v => v.CheckOnStartup).ToArray();
            _scheduledHealthChecks = _healthChecks.Where(v => v.CheckOnSchedule).ToArray();
            _eventDrivenHealthChecks = GetEventDrivenHealthChecks();
            _startupGracePeriodEndTime = runtimeInfo.StartTime + TimeSpan.FromMinutes(15);
        }

        public List<HealthCheck> Results()
        {
            return _healthCheckResults.Values.ToList();
        }

        private Dictionary<Type, IEventDrivenHealthCheck[]> GetEventDrivenHealthChecks()
        {
            return _healthChecks
                .SelectMany(h => h.GetType().GetAttributes<CheckOnAttribute>().Select(a =>
                {
                    var eventDrivenType = typeof(EventDrivenHealthCheck<>).MakeGenericType(a.EventType);
                    var eventDriven = (IEventDrivenHealthCheck)Activator.CreateInstance(eventDrivenType, h, a.Condition);

                    return Tuple.Create(a.EventType, eventDriven);
                }))
                .GroupBy(t => t.Item1, t => t.Item2)
                .ToDictionary(g => g.Key, g => g.ToArray());
        }

        private void ProcessHealthChecks()
        {
            List<IProvideHealthCheck> healthChecks;

            lock (_pendingHealthChecks)
            {
                healthChecks = _pendingHealthChecks.ToList();
                _pendingHealthChecks.Clear();
            }

            _debounce.Pause();

            try
            {
                var results = healthChecks.Select(c =>
                    {
                        _logger.Trace("Check health -> {0}", c.GetType().Name);
                        var result = c.Check();
                        _logger.Trace("Check health <- {0}", c.GetType().Name);

                        return result;
                    })
                    .ToList();

                foreach (var result in results)
                {
                    if (result.Type == HealthCheckResult.Ok)
                    {
                        var previous = _healthCheckResults.Find(result.Source.Name);

                        if (previous != null)
                        {
                            _eventAggregator.PublishEvent(new HealthCheckRestoredEvent(previous, !_hasRunHealthChecksAfterGracePeriod));
                        }

                        _healthCheckResults.Remove(result.Source.Name);
                    }
                    else
                    {
                        if (_healthCheckResults.Find(result.Source.Name) == null)
                        {
                            _eventAggregator.PublishEvent(new HealthCheckFailedEvent(result, !_hasRunHealthChecksAfterGracePeriod));
                        }

                        _healthCheckResults.Set(result.Source.Name, result);
                    }
                }
            }
            finally
            {
                _debounce.Resume();
            }

            _eventAggregator.PublishEvent(new HealthCheckCompleteEvent());
        }

        public void Execute(CheckHealthCommand message)
        {
            var healthChecks = message.Trigger == CommandTrigger.Manual ? _healthChecks  : _scheduledHealthChecks;

            lock (_pendingHealthChecks)
            {
                foreach (var healthCheck in healthChecks)
                {
                    _pendingHealthChecks.Add(healthCheck);
                }
            }

            ProcessHealthChecks();
        }

        public void HandleAsync(ApplicationStartedEvent message)
        {
            lock (_pendingHealthChecks)
            {
                foreach (var healthCheck in _startupHealthChecks)
                {
                    _pendingHealthChecks.Add(healthCheck);
                }
            }

            ProcessHealthChecks();
        }

        public void HandleAsync(IEvent message)
        {
            if (message is HealthCheckCompleteEvent || message is ApplicationStartedEvent)
            {
                return;
            }

            // If we haven't previously re-run health checks after startup grace period run startup checks again and track so they aren't run again.
            // Return early after re-running checks to avoid triggering checks multiple times.

            if (!_hasRunHealthChecksAfterGracePeriod && !_isRunningHealthChecksAfterGracePeriod && DateTime.UtcNow > _startupGracePeriodEndTime)
            {
                _isRunningHealthChecksAfterGracePeriod = true;

                lock (_pendingHealthChecks)
                {
                    foreach (var healthCheck in _startupHealthChecks)
                    {
                        _pendingHealthChecks.Add(healthCheck);
                    }
                }

                // Call it directly so it's not debounced and any alerts can be sent.
                ProcessHealthChecks();

                // Update after running health checks so new failure notifications aren't sent 2x.
                _hasRunHealthChecksAfterGracePeriod = true;

                // Explicitly notify for any failed checks since existing failed results would not have sent events.
                var results = _healthCheckResults.Values.ToList();

                foreach (var result in results)
                {
                    _eventAggregator.PublishEvent(new HealthCheckFailedEvent(result, false));
                }

                _isRunningHealthChecksAfterGracePeriod = false;
            }

            if (!_eventDrivenHealthChecks.TryGetValue(message.GetType(), out var checks))
            {
                return;
            }

            var filteredChecks = new List<IProvideHealthCheck>();
            var healthCheckResults = _healthCheckResults.Values.ToList();

            foreach (var eventDrivenHealthCheck in checks)
            {
                var healthCheckType = eventDrivenHealthCheck.HealthCheck.GetType();
                var previouslyFailed = healthCheckResults.Any(r => r.Source == healthCheckType);

                if (eventDrivenHealthCheck.ShouldExecute(message, previouslyFailed))
                {
                    filteredChecks.Add(eventDrivenHealthCheck.HealthCheck);
                    continue;
                }
            }

            lock (_pendingHealthChecks)
            {
                filteredChecks.ForEach(h => _pendingHealthChecks.Add(h));
            }

            _debounce.Execute();
        }
    }
}
