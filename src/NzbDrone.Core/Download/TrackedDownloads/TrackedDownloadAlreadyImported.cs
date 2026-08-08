using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.History;

namespace NzbDrone.Core.Download.TrackedDownloads
{
    public interface ITrackedDownloadAlreadyImported
    {
        bool IsImported(TrackedDownload trackedDownload, List<EpisodeHistory> historyItems);
    }

    public class TrackedDownloadAlreadyImported : ITrackedDownloadAlreadyImported
    {
        private readonly Logger _logger;

        public TrackedDownloadAlreadyImported(Logger logger)
        {
            _logger = logger;
        }

        public bool IsImported(TrackedDownload trackedDownload, List<EpisodeHistory> historyItems)
        {
            _logger.Trace("Checking if all episodes for '{0}' have been imported", trackedDownload.DownloadItem.Title);

            if (historyItems.Empty())
            {
                _logger.Trace("No history for {0}", trackedDownload.DownloadItem.Title);
                return false;
            }

            // trackedDownload.DownloadItem.DownloadId can be reused/matched by
            // the download client's own duplicate handling across genuinely
            // distinct grab attempts (observed with NZBGet's "found in history
            // with exactly same content" duplicate detection). When that
            // happens, historyItems can contain an OLDER attempt's
            // DownloadFolderImported event for an episode ID that THIS
            // download's files were never actually imported from. Matching by
            // EpisodeId alone then falsely reports the current, un-imported
            // download as already complete - which, combined with
            // removeCompletedDownloads, deletes its files before Sonarr ever
            // attempts to import them. Requiring the history event's own
            // recorded source path to fall under this download's current
            // output path closes that gap: a stale/unrelated import can never
            // satisfy it.
            var outputPath = trackedDownload.ImportItem.OutputPath;

            var allEpisodesImportedInHistory = trackedDownload.RemoteEpisode.Episodes.All(e =>
            {
                var lastHistoryItem = historyItems.FirstOrDefault(h => h.EpisodeId == e.Id);

                if (lastHistoryItem == null)
                {
                    _logger.Trace("No history for episode: S{0:00}E{1:00} [{2}]", e.SeasonNumber, e.EpisodeNumber, e.Id);
                    return false;
                }

                _logger.Trace("Last event for episode: S{0:00}E{1:00} [{2}] is: {3}", e.SeasonNumber, e.EpisodeNumber, e.Id, lastHistoryItem.EventType);

                if (lastHistoryItem.EventType != EpisodeHistoryEventType.DownloadFolderImported)
                {
                    return false;
                }

                var droppedPath = lastHistoryItem.Data.GetValueOrDefault("DroppedPath");

                if (droppedPath.IsNullOrWhiteSpace() || !outputPath.Contains(new OsPath(droppedPath)))
                {
                    _logger.Trace(
                        "Import history for episode: S{0:00}E{1:00} [{2}] does not match this download's current output path '{3}' (history source: '{4}') - treating as not imported",
                        e.SeasonNumber, e.EpisodeNumber, e.Id, outputPath, droppedPath);
                    return false;
                }

                return true;
            });

            _logger.Trace("All episodes for '{0}' have been imported: {1}", trackedDownload.DownloadItem.Title, allEpisodesImportedInHistory);

            return allEpisodesImportedInHistory;
        }
    }
}
