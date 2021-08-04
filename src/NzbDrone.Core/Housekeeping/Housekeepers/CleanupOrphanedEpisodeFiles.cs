using Dapper;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedEpisodeFiles : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedEpisodeFiles(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            using (var mapper = _database.OpenConnection())
            {
                mapper.Execute(@"DELETE FROM EpisodeFiles
                                     WHERE Id IN (
                                     SELECT EpisodeFiles.Id FROM EpisodeFiles
                                     LEFT OUTER JOIN Episodes
                                     ON EpisodeFiles.Id = Episodes.EpisodeFileId
                                     WHERE Episodes.Id IS NULL)");
            }
        }
    }
}
