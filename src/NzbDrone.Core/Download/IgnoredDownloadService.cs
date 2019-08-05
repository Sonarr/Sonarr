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
            var episodes = trackedDownload.RemoteEpisode.Episodes;

            if (series == null || episodes.Empty())
            {
                _logger.Warn("Unable to ignore download for unknown series/episode");
                return false;
            }

            var downloadIgnoredEvent = new DownloadIgnoredEvent
                                      {
                                          SeriesId = series.Id,
                                          EpisodeIds = episodes.Select(e => e.Id).ToList(),
                                          Language = trackedDownload.RemoteEpisode.ParsedEpisodeInfo.Language,
                                          Quality = trackedDownload.RemoteEpisode.ParsedEpisodeInfo.Quality,
                                          SourceTitle = trackedDownload.DownloadItem.Title,
                                          DownloadClient = trackedDownload.DownloadItem.DownloadClient,
                                          DownloadId = trackedDownload.DownloadItem.DownloadId,
                                          Message = "Manually ignored"
                                      };

            _eventAggregator.PublishEvent(downloadIgnoredEvent);
            return true;
        }
    }
}
