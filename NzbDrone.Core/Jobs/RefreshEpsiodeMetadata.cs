using System.Collections.Generic;
using System.Linq;
using System;
using NLog;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Jobs
{
    public class RefreshEpisodeMetadata : IJob
    {
        private readonly MediaFileProvider _mediaFileProvider;
        private readonly SeriesProvider _seriesProvider;
        private readonly MetadataProvider _metadataProvider;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public RefreshEpisodeMetadata(MediaFileProvider mediaFileProvider, SeriesProvider seriesProvider,
                                        MetadataProvider metadataProvider)
        {
            _mediaFileProvider = mediaFileProvider;
            _seriesProvider = seriesProvider;
            _metadataProvider = metadataProvider;
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
                seriesToRefresh = _seriesProvider.All().ToList();

            else
                seriesToRefresh = new List<Series> { _seriesProvider.Get(options.SeriesId) };

            foreach(var series in seriesToRefresh)
            {
                RefreshMetadata(notification, series); 
            }  
        }

        private void RefreshMetadata(ProgressNotification notification, Series series)
        {
            notification.CurrentMessage = String.Format("Refreshing episode metadata for '{0}'", series.Title);

            Logger.Debug("Getting episodes from database for series: {0}", series.SeriesId);
            var episodeFiles = _mediaFileProvider.GetSeriesFiles(series.SeriesId);

            if (episodeFiles == null || episodeFiles.Count == 0)
            {
                Logger.Warn("No episodes in database found for series: {0}", series.SeriesId);
                return;
            }

            try
            {
                _metadataProvider.CreateForEpisodeFiles(episodeFiles.ToList());
            }

            catch (Exception e)
            {
                Logger.WarnException("An error has occurred while refreshing episode metadata", e);
            }

            notification.CurrentMessage = String.Format("Epsiode metadata refresh completed for {0}", series.Title);
        }
    }
}