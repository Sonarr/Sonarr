using Dapper;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupDuplicateMetadataFiles : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupDuplicateMetadataFiles(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            DeleteDuplicateSeriesMetadata();
            DeleteDuplicateEpisodeMetadata();
            DeleteDuplicateEpisodeImages();
        }

        private void DeleteDuplicateSeriesMetadata()
        {
            using (var mapper = _database.OpenConnection())
            {
                mapper.Execute(@"DELETE FROM MetadataFiles
                                     WHERE Id IN (
                                         SELECT Id FROM MetadataFiles
                                         WHERE Type = 1
                                         GROUP BY SeriesId, Consumer
                                         HAVING COUNT(SeriesId) > 1
                                     )");
            }
        }

        private void DeleteDuplicateEpisodeMetadata()
        {
            using (var mapper = _database.OpenConnection())
            {
                mapper.Execute(@"DELETE FROM MetadataFiles
                                     WHERE Id IN (
                                         SELECT Id FROM MetadataFiles
                                         WHERE Type = 2
                                         GROUP BY EpisodeFileId, Consumer
                                         HAVING COUNT(EpisodeFileId) > 1
                                     )");
            }
        }

        private void DeleteDuplicateEpisodeImages()
        {
            using (var mapper = _database.OpenConnection())
            {
                mapper.Execute(@"DELETE FROM MetadataFiles
                                     WHERE Id IN (
                                         SELECT Id FROM MetadataFiles
                                         WHERE Type = 5
                                         GROUP BY EpisodeFileId, Consumer
                                         HAVING COUNT(EpisodeFileId) > 1
                                     )");
            }
        }
    }
}
