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
            var mapper = _database.GetDataMapper();

            mapper.ExecuteNonQuery(@"DELETE FROM MetadataFiles
                                     WHERE Id IN (
                                         SELECT Id FROM MetadataFiles
                                         WHERE RelativePath
                                         LIKE '_:\%'
                                         OR RelativePath
                                         LIKE '\%'
                                         OR RelativePath
                                         LIKE '/%'
                                     )");
        }
    }
}
