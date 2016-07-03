using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Download.Clients.RssGenerator;

namespace NzbDrone.Core.Housekeeping.Housekeepers {

    public class CleanupTorrentRssCache : IHousekeepingTask {
        private static readonly TimeSpan CleanupAge = TimeSpan.FromDays(1);
        private readonly ITorrentRssGeneratorStorage _torrentRssGeneratorStorage;

        public CleanupTorrentRssCache(ITorrentRssGeneratorStorage torrentRssGeneratorStorage) {
            this._torrentRssGeneratorStorage = torrentRssGeneratorStorage;
        }

        public void Clean() {
            var cutoffdate = DateTimeOffset.UtcNow.Subtract(CleanupAge);

            var cleanup = this._torrentRssGeneratorStorage.All()
                .Where(x => x.Status == Download.DownloadItemStatus.Completed && 
                            x.StatusAt <= cutoffdate
                ).Select(x => x.Guid)
                .ToArray();

            foreach(var guid in cleanup)
                this._torrentRssGeneratorStorage.Delete(guid);
        }
    }
}
