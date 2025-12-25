using System;
using System.Threading;
using System.Threading.Tasks;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Instrumentation
{
    public interface ILogRepository : IBasicRepository<Log>
    {
        void Trim();
        Task TrimAsync(CancellationToken cancellationToken = default);
    }

    public class LogRepository : BasicRepository<Log>, ILogRepository
    {
        public LogRepository(ILogDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public void Trim()
        {
            var trimDate = DateTime.UtcNow.AddDays(-7).Date;
            Delete(c => c.Time <= trimDate);
            Vacuum();
        }

        public async Task TrimAsync(CancellationToken cancellationToken = default)
        {
            var trimDate = DateTime.UtcNow.AddDays(-7).Date;
            await DeleteAsync(c => c.Time <= trimDate, cancellationToken).ConfigureAwait(false);
            await VacuumAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
