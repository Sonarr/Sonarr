using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser.Model;
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
        private static readonly int[] EscalationBackOffPeriods = {
                                                                     0,
                                                                     5 * 60,
                                                                     15 * 60,
                                                                     30 * 60,
                                                                     60 * 60,
                                                                     3 * 60 * 60,
                                                                     6 * 60 * 60,
                                                                     12 * 60 * 60,
                                                                     24 * 60 * 60
                                                                 };

        private static readonly int MaximumEscalationLevel = EscalationBackOffPeriods.Length - 1;

        protected readonly object _syncRoot = new object();

        protected readonly IProviderStatusRepository<TModel> _providerStatusRepository;
        protected readonly Logger _logger;

        public ProviderStatusServiceBase(IProviderStatusRepository<TModel> providerStatusRepository, Logger logger)
        {
            _providerStatusRepository = providerStatusRepository;
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

            return TimeSpan.FromSeconds(EscalationBackOffPeriods[level]);
        }

        public virtual void RecordSuccess(int providerId)
        {
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
            }
        }

        protected virtual void RecordFailure(int providerId, TimeSpan minimumBackOff, bool escalate)
        {
            lock (_syncRoot)
            {
                var status = GetProviderStatus(providerId);

                var now = DateTime.UtcNow;

                if (status.EscalationLevel == 0)
                {
                    status.InitialFailure = now;
                }

                status.MostRecentFailure = now;
                if (escalate)
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

                status.DisabledTill = now + CalculateBackOffPeriod(status);

                _providerStatusRepository.Upsert(status);
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
            var providerStatus = _providerStatusRepository.FindByProviderId(message.ProviderId);

            if (providerStatus != null)
            {
                _providerStatusRepository.Delete(providerStatus);
            }
        }
    }
}
