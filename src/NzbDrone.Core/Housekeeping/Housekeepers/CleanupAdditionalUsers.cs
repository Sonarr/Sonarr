using Dapper;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupAdditionalUsers : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupAdditionalUsers(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            using (var mapper = _database.OpenConnection())
            {
                mapper.Execute(@"DELETE FROM Users
                                 WHERE ID NOT IN (
                                 SELECT ID FROM Users
                                 LIMIT 1)");
            }
        }
    }
}
