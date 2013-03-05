using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Jobs.Implementations
{
    public class RefreshEpisodeMetadata : IJob
    {
        private readonly IMediaFileService _mediaFileService;
        private readonly ISeriesRepository _seriesRepository;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public RefreshEpisodeMetadata(IMediaFileService mediaFileService, ISeriesRepository seriesRepository)
        {
            _mediaFileService = mediaFileService;
            _seriesRepository = seriesRepository;
        }

        public string Name
        {
            get { return "Refresh Episode Metadata"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromTicks(0); }
        }

        public void Start(ProgressNotification notification, dynamic options)
        {
            List<Series> seriesToRefresh;

            if (options == null || options.SeriesId <= 0)
                seriesToRefresh = _seriesRepository.All().ToList();

            else
                seriesToRefresh = new List<Series> { _seriesRepository.Get(options.SeriesId) };

            foreach (var series in seriesToRefresh)
            {
                RefreshMetadata(notification, series);
            }
        }

        private void RefreshMetadata(ProgressNotification notification, Series series)
        {
            notification.CurrentMessage = String.Format("Refreshing episode metadata for '{0}'", series.Title);

            Logger.Debug("Getting episodes from database for series: {0}", series.Id);
            var episodeFiles = _mediaFileService.GetFilesBySeries(series.Id);

            if (episodeFiles == null || episodeFiles.Count == 0)
            {
                Logger.Warn("No episodes in database found for series: {0}", series.Id);
                return;
            }

            notification.CurrentMessage = String.Format("Episode metadata refresh completed for {0}", series.Title);
        }
    }
}