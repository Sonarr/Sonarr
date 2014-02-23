using NLog;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedBlacklist : IHousekeepingTask
    {
        private readonly IDatabase _database;
        private readonly Logger _logger;

        public CleanupOrphanedBlacklist(IDatabase database, Logger logger)
        {
            _database = database;
            _logger = logger;
        }

        public void Clean()
        {
            _logger.Trace("Running orphaned blacklist cleanup");

            var mapper = _database.GetDataMapper();

            mapper.ExecuteNonQuery(@"DELETE FROM Blacklist
                                     WHERE Id IN (
                                     SELECT Blacklist.Id FROM Blacklist
                                     LEFT OUTER JOIN Series
                                     ON Blacklist.SeriesId = Series.Id
                                     WHERE Series.Id IS NULL)");
        }
    }
}
