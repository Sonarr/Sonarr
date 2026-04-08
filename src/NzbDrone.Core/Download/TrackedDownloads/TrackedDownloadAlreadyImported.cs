using System.Collections.Generic;
using System.Linq;
using NLog;
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
            _logger.Trace("Checking if all episodes for '{DownloadTitle}' have been imported", trackedDownload.DownloadItem.Title);

            if (historyItems.Empty())
            {
                _logger.Trace("No history for {DownloadTitle}", trackedDownload.DownloadItem.Title);
                return false;
            }

            var allEpisodesImportedInHistory = trackedDownload.RemoteEpisode.Episodes.All(e =>
            {
                var lastHistoryItem = historyItems.FirstOrDefault(h => h.EpisodeId == e.Id);

                if (lastHistoryItem == null)
                {
                    _logger.Trace("No history for episode: S{SeasonNumber:00}E{EpisodeNumber:00} [{EpisodeId}]", e.SeasonNumber, e.EpisodeNumber, e.Id);
                    return false;
                }

                _logger.Trace("Last event for episode: S{SeasonNumber:00}E{EpisodeNumber:00} [{EpisodeId}] is: {EventType}", e.SeasonNumber, e.EpisodeNumber, e.Id, lastHistoryItem.EventType);

                return lastHistoryItem.EventType == EpisodeHistoryEventType.DownloadFolderImported;
            });

            _logger.Trace("All episodes for '{DownloadTitle}' have been imported: {AllImported}", trackedDownload.DownloadItem.Title, allEpisodesImportedInHistory);

            return allEpisodesImportedInHistory;
        }
    }
}
