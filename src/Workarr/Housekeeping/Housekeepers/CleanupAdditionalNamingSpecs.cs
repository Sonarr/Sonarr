using Dapper;
using Workarr.Datastore;

namespace Workarr.Housekeeping.Housekeepers
{
    public class CleanupAdditionalNamingSpecs : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupAdditionalNamingSpecs(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            using var mapper = _database.OpenConnection();
            mapper.Execute(@"DELETE FROM ""NamingConfig""
                                     WHERE ""Id"" NOT IN (
                                     SELECT ""Id"" FROM ""NamingConfig""
                                     LIMIT 1)");
        }
    }
}
