using Dapper;
using Workarr.Datastore;

namespace Workarr.Housekeeping.Housekeepers
{
    public class CleanupAdditionalUsers : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupAdditionalUsers(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            using var mapper = _database.OpenConnection();
            mapper.Execute(@"DELETE FROM ""Users""
                                 WHERE ""Id"" NOT IN (
                                 SELECT ""Id"" FROM ""Users""
                                 LIMIT 1)");
        }
    }
}
