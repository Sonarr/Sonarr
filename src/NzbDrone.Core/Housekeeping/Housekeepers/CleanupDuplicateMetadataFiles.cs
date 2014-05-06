using NLog;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupDuplicateMetadataFiles : IHousekeepingTask
    {
        private readonly IDatabase _database;
        private readonly Logger _logger;

        public CleanupDuplicateMetadataFiles(IDatabase database, Logger logger)
        {
            _database = database;
            _logger = logger;
        }

        public void Clean()
        {
            _logger.Debug("Running cleanup of duplicate metadata files");

            DeleteDuplicateSeriesMetadata();
            DeleteDuplicateEpisodeMetadata();
            DeleteDuplicateEpisodeImages();
        }

        private void DeleteDuplicateSeriesMetadata()
        {
            var mapper = _database.GetDataMapper();

            mapper.ExecuteNonQuery(@"DELETE FROM MetadataFiles
                                     WHERE Id IN (
                                         SELECT Id FROM MetadataFiles
                                         WHERE Type = 1
                                         GROUP BY SeriesId, Consumer
                                         HAVING COUNT(SeriesId) > 1
                                     )");
        }

        private void DeleteDuplicateEpisodeMetadata()
        {
            var mapper = _database.GetDataMapper();

            mapper.ExecuteNonQuery(@"DELETE FROM MetadataFiles
                                     WHERE Id IN (
                                         SELECT Id FROM MetadataFiles
                                         WHERE Type = 2
                                         GROUP BY EpisodeFileId, Consumer
                                         HAVING COUNT(EpisodeFileId) > 1
                                     )");
        }

        private void DeleteDuplicateEpisodeImages()
        {
            var mapper = _database.GetDataMapper();

            mapper.ExecuteNonQuery(@"DELETE FROM MetadataFiles
                                     WHERE Id IN (
                                         SELECT Id FROM MetadataFiles
                                         WHERE Type = 5
                                         GROUP BY EpisodeFileId, Consumer
                                         HAVING COUNT(EpisodeFileId) > 1
                                     )");
        }
    }
}
