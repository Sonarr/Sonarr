using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using Ninject;

namespace NzbDrone.Core.Instrumentation
{
    public class LogDbContext : DbContext
    {
        [Inject]
        public LogDbContext(DbConnection connection)
            : base(connection, false)
        {
        }

        public LogDbContext()
        {
        }

        public DbSet<Log> Logs { get; set; }
    }
}
