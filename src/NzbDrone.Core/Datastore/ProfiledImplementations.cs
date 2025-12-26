using System.Data.Common;
using StackExchange.Profiling.Data;

namespace NzbDrone.Core.Datastore;

public static class ProfiledImplementations
{
    public class NpgSqlConnection : ProfiledDbConnection
    {
        public NpgSqlConnection(DbConnection connection, IDbProfiler profiler)
            : base(connection, profiler)
        {
        }
    }
}
