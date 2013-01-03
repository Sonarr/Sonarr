using System.Data.Common;
using System.Data.Entity;
using System.Linq;

namespace NzbDrone.Core.Instrumentation
{
    public class LogDbContext : DbContext
    {
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
