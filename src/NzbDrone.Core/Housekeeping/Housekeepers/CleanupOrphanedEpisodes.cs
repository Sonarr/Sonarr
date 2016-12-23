using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedEpisodes : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedEpisodes(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            var mapper = _database.GetDataMapper();

            mapper.ExecuteNonQuery(@"DELETE FROM Episodes
                                     WHERE Id IN (
                                     SELECT Episodes.Id FROM Episodes
                                     LEFT OUTER JOIN Series
                                     ON Episodes.SeriesId = Series.Id
                                     WHERE Series.Id IS NULL)");
        }
    }
}
