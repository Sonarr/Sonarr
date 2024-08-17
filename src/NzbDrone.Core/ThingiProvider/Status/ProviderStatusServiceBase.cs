using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider.Events;

namespace NzbDrone.Core.ThingiProvider.Status
{
    public interface IProviderStatusServiceBase<TModel>
        where TModel : ProviderStatusBase, new()
    {
        List<TModel> GetBlockedProviders();
        void RecordSuccess(int providerId);
        void RecordFailure(int providerId, TimeSpan minimumBackOff = default(TimeSpan));
        void RecordConnectionFailure(int providerId);
    }

    public abstract class ProviderStatusServiceBase<TProvider, TModel> : IProviderStatusServiceBase<TModel>, IHandleAsync<ProviderDeletedEvent<TProvider>>
        where TProvider : IProvider
        where TModel : ProviderStatusBase, new()
    {
        protected readonly object _syncRoot = new object();

        protected readonly IProviderStatusRepository<TModel> _providerStatusRepository;
        protected readonly IEventAggregator _eventAggregator;
        protected readonly IRuntimeInfo _runtimeInfo;
        protected readonly Logger _logger;

        protected int MaximumEscalationLevel { get; set; } = EscalationBackOff.Periods.Length - 1;
        protected TimeSpan MinimumTimeSinceInitialFailure { get; set; } = TimeSpan.Zero;
        protected TimeSpan MinimumTimeSinceStartup { get; set; } = TimeSpan.FromMinutes(15);

        public ProviderStatusServiceBase(IProviderStatusRepository<TModel> providerStatusRepository, IEventAggregator eventAggregator, IRuntimeInfo runtimeInfo, Logger logger)
        {
            _providerStatusRepository = providerStatusRepository;
            _eventAggregator = eventAggregator;
            _runtimeInfo = runtimeInfo;
            _logger = logger;
        }

        public virtual List<TModel> GetBlockedProviders()
        {
            return _providerStatusRepository.All().Where(v => v.IsDisabled()).ToList();
        }

        protected virtual TModel GetProviderStatus(int providerId)
        {
            return _providerStatusRepository.FindByProviderId(providerId) ?? new TModel { ProviderId = providerId };
        }

        protected virtual TimeSpan CalculateBackOffPeriod(TModel status)
        {
            var level = Math.Min(MaximumEscalationLevel, status.EscalationLevel);

            return TimeSpan.FromSeconds(EscalationBackOff.Periods[level]);
        }

        public virtual void RecordSuccess(int providerId)
        {
            if (providerId <= 0)
            {
                return;
            }

            lock (_syncRoot)
            {
                var status = GetProviderStatus(providerId);

                if (status.EscalationLevel == 0)
                {
                    return;
                }

                status.EscalationLevel--;
                status.DisabledTill = null;

                _providerStatusRepository.Upsert(status);

                _eventAggregator.PublishEvent(new ProviderStatusChangedEvent<TProvider>(providerId, status));
            }
        }

        protected virtual void RecordFailure(int providerId, TimeSpan minimumBackOff, bool escalate)
        {
            if (providerId <= 0)
            {
                return;
            }

            lock (_syncRoot)
            {
                var status = GetProviderStatus(providerId);

                var now = DateTime.UtcNow;
                status.MostRecentFailure = now;

                if (status.EscalationLevel == 0)
                {
                    status.InitialFailure = now;
                    status.EscalationLevel = 1;
                    escalate = false;
                }

                var inStartupGracePeriod = (_runtimeInfo.StartTime + MinimumTimeSinceStartup) > now;
                var inGracePeriod = (status.InitialFailure.Value + MinimumTimeSinceInitialFailure) > now;

                if (escalate && !inGracePeriod && !inStartupGracePeriod)
                {
                    status.EscalationLevel = Math.Min(MaximumEscalationLevel, status.EscalationLevel + 1);
                }

                if (minimumBackOff != TimeSpan.Zero)
                {
                    while (status.EscalationLevel < MaximumEscalationLevel && CalculateBackOffPeriod(status) < minimumBackOff)
                    {
                        status.EscalationLevel++;
                    }
                }

                if (!inGracePeriod || minimumBackOff != TimeSpan.Zero)
                {
                    status.DisabledTill = now + CalculateBackOffPeriod(status);
                }

                if (inStartupGracePeriod && minimumBackOff == TimeSpan.Zero && status.DisabledTill.HasValue)
                {
                    var maximumDisabledTill = now + TimeSpan.FromSeconds(EscalationBackOff.Periods[2]);
                    if (maximumDisabledTill < status.DisabledTill)
                    {
                        status.DisabledTill = maximumDisabledTill;
                    }
                }

                _providerStatusRepository.Upsert(status);

                _eventAggregator.PublishEvent(new ProviderStatusChangedEvent<TProvider>(providerId, status));
            }
        }

        public virtual void RecordFailure(int providerId, TimeSpan minimumBackOff = default(TimeSpan))
        {
            RecordFailure(providerId, minimumBackOff, true);
        }

        public virtual void RecordConnectionFailure(int providerId)
        {
            RecordFailure(providerId, default(TimeSpan), false);
        }

        public virtual void HandleAsync(ProviderDeletedEvent<TProvider> message)
        {
            _providerStatusRepository.DeleteByProviderId(message.ProviderId);
        }
    }
}
