using Dapper;
using Workarr.Datastore;

namespace Workarr.Housekeeping.Housekeepers
{
    public class CleanupOrphanedPendingReleases : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedPendingReleases(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            using var mapper = _database.OpenConnection();
            mapper.Execute(@"DELETE FROM ""PendingReleases""
                                     WHERE ""Id"" IN (
                                     SELECT ""PendingReleases"".""Id"" FROM ""PendingReleases""
                                     LEFT OUTER JOIN ""Series""
                                     ON ""PendingReleases"".""SeriesId"" = ""Series"".""Id""
                                     WHERE ""Series"".""Id"" IS NULL)");
        }
    }
}
