using NLog;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupAdditionalNamingSpecs : IHousekeepingTask
    {
        private readonly IDatabase _database;
        private readonly Logger _logger;

        public CleanupAdditionalNamingSpecs(IDatabase database, Logger logger)
        {
            _database = database;
            _logger = logger;
        }

        public void Clean()
        {
            _logger.Debug("Running naming spec cleanup");

            var mapper = _database.GetDataMapper();

            mapper.ExecuteNonQuery(@"DELETE FROM NamingConfig
                                     WHERE ID NOT IN (
                                     SELECT ID FROM NamingConfig
                                     LIMIT 1)");
        }
    }
}
