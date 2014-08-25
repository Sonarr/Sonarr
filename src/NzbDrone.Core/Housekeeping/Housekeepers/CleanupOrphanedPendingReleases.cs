using NLog;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedPendingReleases : IHousekeepingTask
    {
        private readonly IDatabase _database;
        private readonly Logger _logger;

        public CleanupOrphanedPendingReleases(IDatabase database, Logger logger)
        {
            _database = database;
            _logger = logger;
        }

        public void Clean()
        {
            _logger.Debug("Running orphaned pending releases cleanup");

            var mapper = _database.GetDataMapper();

            mapper.ExecuteNonQuery(@"DELETE FROM PendingReleases
                                     WHERE Id IN (
                                     SELECT PendingReleases.Id FROM PendingReleases
                                     LEFT OUTER JOIN Series
                                     ON PendingReleases.SeriesId = Series.Id
                                     WHERE Series.Id IS NULL)");
        }
    }
}
