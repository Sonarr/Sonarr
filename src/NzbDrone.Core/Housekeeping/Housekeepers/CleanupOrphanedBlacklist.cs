using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedBlacklist : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedBlacklist(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            var mapper = _database.GetDataMapper();

            mapper.ExecuteNonQuery(@"DELETE FROM Blacklist
                                     WHERE Id IN (
                                     SELECT Blacklist.Id FROM Blacklist
                                     LEFT OUTER JOIN Series
                                     ON Blacklist.SeriesId = Series.Id
                                     WHERE Series.Id IS NULL AND Blacklist.MovieId = 0)");

            mapper.ExecuteNonQuery(@"DELETE FROM Blacklist
                                     WHERE Id IN (
                                     SELECT Blacklist.Id FROM Blacklist
                                     LEFT OUTER JOIN Movies
                                     ON Blacklist.MovieId = Movies.Id
                                     WHERE Movies.Id IS NULL AND Blacklist.SeriesId = 0)");
        }
    }
}
