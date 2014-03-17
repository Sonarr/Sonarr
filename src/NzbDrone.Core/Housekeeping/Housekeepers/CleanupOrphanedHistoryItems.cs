using NLog;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedHistoryItems : IHousekeepingTask
    {
        private readonly IDatabase _database;
        private readonly Logger _logger;

        public CleanupOrphanedHistoryItems(IDatabase database, Logger logger)
        {
            _database = database;
            _logger = logger;
        }

        public void Clean()
        {
            _logger.Debug("Running orphaned history cleanup");
            CleanupOrphanedBySeries();
            CleanupOrphanedByEpisode();
        }

        private void CleanupOrphanedBySeries()
        {
            var mapper = _database.GetDataMapper();

            mapper.ExecuteNonQuery(@"DELETE FROM History
                                     WHERE Id IN (
                                     SELECT History.Id FROM History
                                     LEFT OUTER JOIN Series
                                     ON History.SeriesId = Series.Id
                                     WHERE Series.Id IS NULL)");
        }

        private void CleanupOrphanedByEpisode()
        {
            var mapper = _database.GetDataMapper();

            mapper.ExecuteNonQuery(@"DELETE FROM History
                                     WHERE Id IN (
                                     SELECT History.Id FROM History
                                     LEFT OUTER JOIN Episodes
                                     ON History.EpisodeId = Episodes.Id
                                     WHERE Episodes.Id IS NULL)");
        }
    }
}
