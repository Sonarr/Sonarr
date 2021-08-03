using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.ThingiProvider.Status;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public abstract class FixFutureProviderStatusTimes<TModel>
        where TModel : ProviderStatusBase, new()
    {
        private readonly IProviderStatusRepository<TModel> _repo;

        protected FixFutureProviderStatusTimes(IProviderStatusRepository<TModel> repo)
        {
            _repo = repo;
        }

        public void Clean()
        {
            var now = DateTime.UtcNow;
            var statuses = _repo.All().ToList();
            var toUpdate = new List<TModel>();

            foreach (var status in statuses)
            {
                var updated = false;
                var escalationDelay = EscalationBackOff.Periods[status.EscalationLevel];
                var disabledTill = now.AddMinutes(escalationDelay);

                if (status.DisabledTill > disabledTill)
                {
                    status.DisabledTill = disabledTill;
                    updated = true;
                }

                if (status.InitialFailure > now)
                {
                    status.InitialFailure = now;
                    updated = true;
                }

                if (status.MostRecentFailure > now)
                {
                    status.MostRecentFailure = now;
                    updated = true;
                }

                if (updated)
                {
                    toUpdate.Add(status);
                }
            }

            _repo.UpdateMany(toUpdate);
        }
    }
}
