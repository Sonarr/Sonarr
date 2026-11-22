using Dapper;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupDuplicateExtraFiles : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupDuplicateExtraFiles(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            using var mapper = _database.OpenConnection();
            mapper.Execute(@"DELETE FROM ""ExtraFiles""
                                     WHERE ""Id"" NOT IN (
                                         SELECT MAX(""Id"") FROM ""ExtraFiles""
                                         GROUP BY ""SeriesId"", ""RelativePath""
                                     )");
        }
    }
}
