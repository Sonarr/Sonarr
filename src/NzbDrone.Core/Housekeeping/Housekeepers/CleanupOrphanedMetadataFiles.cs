using NLog;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedMetadataFiles : IHousekeepingTask
    {
        private readonly IDatabase _database;
        private readonly Logger _logger;

        public CleanupOrphanedMetadataFiles(IDatabase database, Logger logger)
        {
            _database = database;
            _logger = logger;
        }

        public void Clean()
        {
            _logger.Trace("Running orphaned episode files cleanup");

            DeleteOrphanedBySeries();
            DeleteOrphanedByEpisodeFile();
        }

        private void DeleteOrphanedBySeries()
        {
            var mapper = _database.GetDataMapper();

            mapper.ExecuteNonQuery(@"DELETE FROM MetadataFiles
                                     WHERE Id IN (
                                     SELECT MetadataFiles.Id FROM MetadataFiles
                                     LEFT OUTER JOIN Series
                                     ON MetadataFiles.SeriesId = Series.Id
                                     WHERE Series.Id IS NULL)");
        }

        private void DeleteOrphanedByEpisodeFile()
        {
            var mapper = _database.GetDataMapper();

            mapper.ExecuteNonQuery(@"DELETE FROM MetadataFiles
                                     WHERE Id IN (
                                     SELECT MetadataFiles.Id FROM MetadataFiles
                                     LEFT OUTER JOIN EpisodeFiles
                                     ON MetadataFiles.EpisodeFileId = EpisodeFiles.Id
                                     WHERE MetadataFiles.EpisodeFileId > 0
                                     AND EpisodeFiles.Id IS NULL)");
        }
    }
}
