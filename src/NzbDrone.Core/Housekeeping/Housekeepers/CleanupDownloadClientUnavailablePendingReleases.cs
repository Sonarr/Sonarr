using System;
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
            var mapper = _database.GetDataMapper();
            var twoWeeksAgo = DateTime.UtcNow.AddDays(-14);

            mapper.Delete<PendingRelease>(p => p.Added < twoWeeksAgo &&
                                               (p.Reason == PendingReleaseReason.DownloadClientUnavailable ||
                                                p.Reason == PendingReleaseReason.Fallback));

//            mapper.AddParameter("twoWeeksAgo", $"{DateTime.UtcNow.AddDays(-14).ToString("s")}Z");

//            mapper.ExecuteNonQuery(@"DELETE FROM PendingReleases
//                                     WHERE Added < @twoWeeksAgo
//                                     AND (Reason = 'DownloadClientUnavailable' OR Reason = 'Fallback')");
        }
    }
}
