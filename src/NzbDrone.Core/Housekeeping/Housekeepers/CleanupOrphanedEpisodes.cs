using NLog;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedEpisodes : IHousekeepingTask
    {
        private readonly IDatabase _database;
        private readonly Logger _logger;

        public CleanupOrphanedEpisodes(IDatabase database, Logger logger)
        {
            _database = database;
            _logger = logger;
        }

        public void Clean()
        {
            _logger.Debug("Running orphaned episodes cleanup");

            var mapper = _database.GetDataMapper();

            mapper.ExecuteNonQuery(@"DELETE FROM Episodes
                                     WHERE Id IN (
                                     SELECT Episodes.Id FROM Episodes
                                     LEFT OUTER JOIN Series
                                     ON Episodes.SeriesId = Series.Id
                                     WHERE Series.Id IS NULL)");
        }
    }
}
