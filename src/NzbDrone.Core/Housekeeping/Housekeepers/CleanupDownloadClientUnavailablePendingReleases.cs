using System;
using Dapper;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Download.Pending;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupDownloadClientUnavailablePendingReleases : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupDownloadClientUnavailablePendingReleases(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            var mapper = _database.OpenConnection();

            mapper.Execute(@"DELETE FROM PendingReleases
                            WHERE Added < @TwoWeeksAgo
                            AND REASON IN @Reasons",
                          new
                          {
                              TwoWeeksAgo = DateTime.UtcNow.AddDays(-14),
                              Reasons = new[] { (int)PendingReleaseReason.DownloadClientUnavailable, (int)PendingReleaseReason.Fallback }
                          });
        }
    }
}
