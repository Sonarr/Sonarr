using Dapper;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedNotificationStatus : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedNotificationStatus(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            using var mapper = _database.OpenConnection();

            mapper.Execute(@"DELETE FROM ""NotificationStatus""
                                     WHERE ""Id"" IN (
                                     SELECT ""NotificationStatus"".""Id"" FROM ""NotificationStatus""
                                     LEFT OUTER JOIN ""Notifications""
                                     ON ""NotificationStatus"".""ProviderId"" = ""Notifications"".""Id""
                                     WHERE ""Notifications"".""Id"" IS NULL)");
        }
    }
}
