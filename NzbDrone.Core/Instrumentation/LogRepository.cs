using System;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging;


namespace NzbDrone.Core.Instrumentation
{
    public interface ILogRepository : IBasicRepository<Log>
    {
        void Trim();
    }

    public class LogRepository : BasicRepository<Log>, ILogRepository
    {
        public LogRepository(IDatabase database, IMessageAggregator messageAggregator)
            : base(database, messageAggregator)
        {
        }

        public void Trim()
        {
            var trimDate = DateTime.UtcNow.AddDays(-7).Date;
            Delete(c => c.Time <= trimDate);
        }
    }
}