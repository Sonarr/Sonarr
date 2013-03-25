using System;
using System.Data;
using System.Linq;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Instrumentation
{
    public interface ILogRepository : IBasicRepository<Log>
    {
        void Trim();
    }

    public class LogRepository : BasicRepository<Log>, ILogRepository
    {
        public LogRepository(IDatabase database)
            : base(database)
        {
        }

        public void Trim()
        {
            var oldIds = Queryable().Where(c => c.Time < DateTime.Now.AddDays(-30).Date).Select(c => c.Id);
            DeleteMany(oldIds);
        }
    }
}