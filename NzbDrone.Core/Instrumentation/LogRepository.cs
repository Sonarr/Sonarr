using System;
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
        public LogRepository(IObjectDatabase objectDatabase)
            : base(objectDatabase)
        {
        }

        public void Trim()
        {
            var oldIds = Queryable.Where(c => c.Time < DateTime.Now.AddDays(-30).Date).Select(c => c.OID);
            DeleteMany(oldIds);
        }
    }
}