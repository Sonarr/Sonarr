using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Update.History
{
    public interface IUpdateHistoryRepository : IBasicRepository<UpdateHistory>
    {
        Task<UpdateHistory> LastInstalledAsync(CancellationToken cancellationToken = default);
        Task<UpdateHistory> PreviouslyInstalledAsync(CancellationToken cancellationToken = default);
        Task<List<UpdateHistory>> InstalledSinceAsync(DateTime dateTime, CancellationToken cancellationToken = default);
    }

    public class UpdateHistoryRepository : BasicRepository<UpdateHistory>, IUpdateHistoryRepository
    {
        public UpdateHistoryRepository(ILogDatabase logDatabase, IEventAggregator eventAggregator)
            : base(logDatabase, eventAggregator)
        {
        }

        public async Task<UpdateHistory> LastInstalledAsync(CancellationToken cancellationToken = default)
        {
            var results = await QueryAsync(v => v.EventType == UpdateHistoryEventType.Installed, cancellationToken).ConfigureAwait(false);
            var history = results.OrderByDescending(v => v.Date)
                                 .Take(1)
                                 .FirstOrDefault();

            return history;
        }

        public async Task<UpdateHistory> PreviouslyInstalledAsync(CancellationToken cancellationToken = default)
        {
            var results = await QueryAsync(v => v.EventType == UpdateHistoryEventType.Installed, cancellationToken).ConfigureAwait(false);
            var history = results.OrderByDescending(v => v.Date)
                                 .Skip(1)
                                 .Take(1)
                                 .FirstOrDefault();

            return history;
        }

        public async Task<List<UpdateHistory>> InstalledSinceAsync(DateTime dateTime, CancellationToken cancellationToken = default)
        {
            var results = await QueryAsync(v => v.EventType == UpdateHistoryEventType.Installed && v.Date >= dateTime, cancellationToken).ConfigureAwait(false);
            var history = results.OrderBy(v => v.Date)
                                 .ToList();

            return history;
        }
    }
}
