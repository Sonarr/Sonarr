using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedImportListStatus : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedImportListStatus(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            var mapper = _database.GetDataMapper();

            mapper.ExecuteNonQuery(@"DELETE FROM ImportListStatus
                                     WHERE Id IN (
                                     SELECT ImportListStatus.Id FROM ImportListStatus
                                     LEFT OUTER JOIN ImportLists
                                     ON ImportListStatus.ProviderId = ImportLists.Id
                                     WHERE ImportLists.Id IS NULL)");
        }
    }
}
