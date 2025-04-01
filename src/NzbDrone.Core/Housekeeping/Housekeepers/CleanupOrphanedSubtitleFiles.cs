using Dapper;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedSubtitleFiles : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedSubtitleFiles(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            DeleteOrphanedBySeries();
            DeleteOrphanedByEpisodeFile();
            DeleteWhereEpisodeFileIsZero();
        }

        private void DeleteOrphanedBySeries()
        {
            using var mapper = _database.OpenConnection();
            mapper.Execute(@"DELETE FROM ""SubtitleFiles""
                                     WHERE ""Id"" IN (
                                     SELECT ""SubtitleFiles"".""Id"" FROM ""SubtitleFiles""
                                     LEFT OUTER JOIN ""Series""
                                     ON ""SubtitleFiles"".""SeriesId"" = ""Series"".""Id""
                                     WHERE ""Series"".""Id"" IS NULL)");
        }

        private void DeleteOrphanedByEpisodeFile()
        {
            using var mapper = _database.OpenConnection();
            mapper.Execute(@"DELETE FROM ""SubtitleFiles""
                                     WHERE ""Id"" IN (
                                     SELECT ""SubtitleFiles"".""Id"" FROM ""SubtitleFiles""
                                     LEFT OUTER JOIN ""EpisodeFiles""
                                     ON ""SubtitleFiles"".""EpisodeFileId"" = ""EpisodeFiles"".""Id""
                                     WHERE ""SubtitleFiles"".""EpisodeFileId"" > 0
                                     AND ""EpisodeFiles"".""Id"" IS NULL)");
        }

        private void DeleteWhereEpisodeFileIsZero()
        {
            using var mapper = _database.OpenConnection();
            mapper.Execute(@"DELETE FROM ""SubtitleFiles""
                                     WHERE ""Id"" IN (
                                     SELECT ""Id"" FROM ""SubtitleFiles""
                                     WHERE ""EpisodeFileId"" = 0)");
        }
    }
}
