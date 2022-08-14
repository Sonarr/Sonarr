using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Download
{
    public interface IIgnoredDownloadService
    {
        bool IgnoreDownload(TrackedDownload trackedDownload);
    }

    public class IgnoredDownloadService : IIgnoredDownloadService
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public IgnoredDownloadService(IEventAggregator eventAggregator,
                                      Logger logger)
        {
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public bool IgnoreDownload(TrackedDownload trackedDownload)
        {
            var series = trackedDownload.RemoteEpisode.Series;

            if (series == null)
            {
                _logger.Warn("Unable to ignore download for unknown series");
                return false;
            }

            var episodes = trackedDownload.RemoteEpisode.Episodes;

            var downloadIgnoredEvent = new DownloadIgnoredEvent
                                      {
                                          SeriesId = series.Id,
                                          EpisodeIds = episodes.Select(e => e.Id).ToList(),
                                          Languages = trackedDownload.RemoteEpisode.ParsedEpisodeInfo.Languages,
                                          Quality = trackedDownload.RemoteEpisode.ParsedEpisodeInfo.Quality,
                                          SourceTitle = trackedDownload.DownloadItem.Title,
                                          DownloadClientInfo = trackedDownload.DownloadItem.DownloadClientInfo,
                                          DownloadId = trackedDownload.DownloadItem.DownloadId,
                                          TrackedDownload = trackedDownload,
                                          Message = "Manually ignored"
                                      };

            _eventAggregator.PublishEvent(downloadIgnoredEvent);
            return true;
        }
    }
}
