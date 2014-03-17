using NLog;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedEpisodeFiles : IHousekeepingTask
    {
        private readonly IDatabase _database;
        private readonly Logger _logger;

        public CleanupOrphanedEpisodeFiles(IDatabase database, Logger logger)
        {
            _database = database;
            _logger = logger;
        }

        public void Clean()
        {
            _logger.Debug("Running orphaned episode files cleanup");

            var mapper = _database.GetDataMapper();

            mapper.ExecuteNonQuery(@"DELETE FROM EpisodeFiles
                                     WHERE Id IN (
                                     SELECT EpisodeFiles.Id FROM EpisodeFiles
                                     LEFT OUTER JOIN Episodes
                                     ON EpisodeFiles.Id = Episodes.EpisodeFileId
                                     WHERE Episodes.Id IS NULL)");
        }
    }
}
