using System;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Core.Model.Notification;

namespace NzbDrone.Core.Providers.Jobs
{
    public class NewSeriesUpdate : IJob
    {
        private readonly SeriesProvider _seriesProvider;
        private readonly EpisodeProvider _episodeProvider;
        private readonly MediaFileProvider _mediaFileProvider;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public NewSeriesUpdate(SeriesProvider seriesProvider, EpisodeProvider episodeProvider, MediaFileProvider mediaFileProvider)
        {
            _seriesProvider = seriesProvider;
            _episodeProvider = episodeProvider;
            _mediaFileProvider = mediaFileProvider;
        }

        public string Name
        {
            get { return "New Series Update"; }
        }

        public int DefaultInterval
        {
            get { return 1; }
        }

        public void Start(ProgressNotification notification, int targetId)
        {
            ScanSeries(notification);
        }

        private void ScanSeries(ProgressNotification notification)
        {
            var syncList = _seriesProvider.GetAllSeries().Where(s => s.LastInfoSync == null).ToList();
            if (syncList.Count == 0) return;

            foreach (var currentSeries in syncList)
            {
                try
                {
                    notification.CurrentStatus = String.Format("Searching For: {0}", new DirectoryInfo(currentSeries.Path).Name);
                    var updatedSeries = _seriesProvider.UpdateSeriesInfo(currentSeries.SeriesId);

                    notification.CurrentStatus = String.Format("Downloading episode info For: {0}",
                                                                          updatedSeries.Title);
                    _episodeProvider.RefreshEpisodeInfo(updatedSeries.SeriesId);

                    notification.CurrentStatus = String.Format("Scanning disk for {0} files",
                                                                          updatedSeries.Title);
                    _mediaFileProvider.Scan(_seriesProvider.GetSeries(updatedSeries.SeriesId));
                }

                catch (Exception e)
                {
                    Logger.ErrorException(e.Message, e);
                }
            }

            //Keep scanning until there no more shows left.
            ScanSeries(notification);
        }

    }
}