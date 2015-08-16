using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedPendingReleases : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedPendingReleases(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            var mapper = _database.GetDataMapper();

            mapper.ExecuteNonQuery(@"DELETE FROM PendingReleases
                                     WHERE Id IN (
                                     SELECT PendingReleases.Id FROM PendingReleases
                                     LEFT OUTER JOIN Series
                                     ON PendingReleases.SeriesId = Series.Id
                                     WHERE Series.Id IS NULL AND PendingReleases.MovieId = 0)");

            mapper.ExecuteNonQuery(@"DELETE FROM PendingReleases
                                     WHERE Id IN (
                                     SELECT PendingReleases.Id FROM PendingReleases
                                     LEFT OUTER JOIN Movies
                                     ON PendingReleases.MovieId = Movies.Id
                                     WHERE Movies.Id IS NULL AND PendingReleases.SeriesId = 0)");
        }
    }
}
