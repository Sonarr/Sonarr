using Dapper;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedHistoryItems : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedHistoryItems(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            CleanupOrphanedBySeries();
            CleanupOrphanedByEpisode();
        }

        private void CleanupOrphanedBySeries()
        {
            using (var mapper = _database.OpenConnection())
            {
                mapper.Execute(@"DELETE FROM History
                                     WHERE Id IN (
                                     SELECT History.Id FROM History
                                     LEFT OUTER JOIN Series
                                     ON History.SeriesId = Series.Id
                                     WHERE Series.Id IS NULL)");
            }
        }

        private void CleanupOrphanedByEpisode()
        {
            using (var mapper = _database.OpenConnection())
            {
                mapper.Execute(@"DELETE FROM History
                                     WHERE Id IN (
                                     SELECT History.Id FROM History
                                     LEFT OUTER JOIN Episodes
                                     ON History.EpisodeId = Episodes.Id
                                     WHERE Episodes.Id IS NULL)");
            }
        }
    }
}
