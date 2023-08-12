using Dapper;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupAbsolutePathMetadataFiles : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupAbsolutePathMetadataFiles(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            using var mapper = _database.OpenConnection();
            if (_database.DatabaseType == DatabaseType.PostgreSQL)
            {
                mapper.Execute(@"DELETE FROM ""MetadataFiles""
                                    WHERE ""Id"" = ANY (
                                        SELECT ""Id"" FROM ""MetadataFiles""
                                        WHERE ""RelativePath""
                                        LIKE '_:\\%'
                                        OR ""RelativePath""
                                        LIKE '\\%'
                                        OR ""RelativePath""
                                        LIKE '/%'
                                    )");
            }
            else
            {
                mapper.Execute(@"DELETE FROM ""MetadataFiles""
                                    WHERE ""Id"" IN (
                                        SELECT ""Id"" FROM ""MetadataFiles""
                                        WHERE ""RelativePath""
                                        LIKE '_:\%'
                                        OR ""RelativePath""
                                        LIKE '\%'
                                        OR ""RelativePath""
                                        LIKE '/%'
                                    )");
            }
        }
    }
}
