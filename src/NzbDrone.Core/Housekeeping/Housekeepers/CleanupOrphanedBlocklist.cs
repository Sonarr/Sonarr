using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedBlocklist : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedBlocklist(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            var mapper = _database.GetDataMapper();

            mapper.ExecuteNonQuery(@"DELETE FROM Blocklist
                                     WHERE Id IN (
                                     SELECT Blocklist.Id FROM Blocklist
                                     LEFT OUTER JOIN Series
                                     ON Blocklist.SeriesId = Series.Id
                                     WHERE Series.Id IS NULL)");
        }
    }
}
