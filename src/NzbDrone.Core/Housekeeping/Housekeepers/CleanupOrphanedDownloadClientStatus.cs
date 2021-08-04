using Dapper;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedDownloadClientStatus : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedDownloadClientStatus(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            var mapper = _database.OpenConnection();

            mapper.Execute(@"DELETE FROM DownloadClientStatus
                                     WHERE Id IN (
                                     SELECT DownloadClientStatus.Id FROM DownloadClientStatus
                                     LEFT OUTER JOIN DownloadClients
                                     ON DownloadClientStatus.ProviderId = DownloadClients.Id
                                     WHERE DownloadClients.Id IS NULL)");
        }
    }
}
