using System;
using System.Data;
using System.Linq;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Datastore;

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
            var oldIds = Query.Where(c => c.Time < DateTime.Now.AddDays(-30).Date).Select(c => c.Id);
            DeleteMany(oldIds);
        }

        protected override void PublishModelEvent(Log model, Datastore.Events.RepositoryAction action)
        {
            //Don't publish log added events.
        }
    }
}