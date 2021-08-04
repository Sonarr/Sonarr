using Dapper;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedIndexerStatus : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedIndexerStatus(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            using (var mapper = _database.OpenConnection())
            {
                mapper.Execute(@"DELETE FROM IndexerStatus
                                     WHERE Id IN (
                                     SELECT IndexerStatus.Id FROM IndexerStatus
                                     LEFT OUTER JOIN Indexers
                                     ON IndexerStatus.ProviderId = Indexers.Id
                                     WHERE Indexers.Id IS NULL)");
            }
        }
    }
}
