using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupAdditionalNamingSpecs : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupAdditionalNamingSpecs(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            var mapper = _database.GetDataMapper();

            mapper.ExecuteNonQuery(@"DELETE FROM NamingConfig
                                     WHERE ID NOT IN (
                                     SELECT ID FROM NamingConfig
                                     LIMIT 1)");
        }
    }
}
