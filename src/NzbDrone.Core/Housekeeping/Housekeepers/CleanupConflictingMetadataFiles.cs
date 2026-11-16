using Dapper;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupConflictingMetadataFiles : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupConflictingMetadataFiles(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            using var mapper = _database.OpenConnection();
            mapper.Execute(@"DELETE FROM ""MetadataFiles""
                                     WHERE ""Type"" = 2
                                     AND EXISTS (
                                         SELECT 1 FROM ""MetadataFiles"" AS ""Other""
                                         WHERE ""Other"".""Type"" = 2
                                         AND ""Other"".""SeriesId"" = ""MetadataFiles"".""SeriesId""
                                         AND ""Other"".""RelativePath"" = ""MetadataFiles"".""RelativePath""
                                         AND ""Other"".""Consumer"" <> ""MetadataFiles"".""Consumer""
                                     )");
        }
    }
}
