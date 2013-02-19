using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;

namespace NzbDrone.Core.Jobs
{
    /// <summary>
    /// This job processes newly added jobs by downloading their info
    /// from TheTVDB.org and doing a disk scan. this job should only 
    /// </summary>
    public class ImportNewSeriesJob : IJob
    {
        private readonly SeriesProvider _seriesProvider;
        private readonly EpisodeProvider _episodeProvider;
        private readonly MediaFileProvider _mediaFileProvider;
        private readonly UpdateInfoJob _updateInfoJob;
        private readonly DiskScanJob _diskScanJob;
        private readonly BannerDownloadJob _bannerDownloadJob;
        private readonly SeasonProvider _seasonProvider;
        private readonly XemUpdateJob _xemUpdateJob;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private List<int> _attemptedSeries;

        public ImportNewSeriesJob(SeriesProvider seriesProvider, EpisodeProvider episodeProvider,
                                    MediaFileProvider mediaFileProvider, UpdateInfoJob updateInfoJob,
                                    DiskScanJob diskScanJob, BannerDownloadJob bannerDownloadJob,
                                    SeasonProvider seasonProvider, XemUpdateJob xemUpdateJob)
        {
            _seriesProvider = seriesProvider;
            _episodeProvider = episodeProvider;
            _mediaFileProvider = mediaFileProvider;
            _updateInfoJob = updateInfoJob;
            _diskScanJob = diskScanJob;
            _bannerDownloadJob = bannerDownloadJob;
            _seasonProvider = seasonProvider;
            _xemUpdateJob = xemUpdateJob;
        }

        public string Name
        {
            get { return "Import New Series"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromHours(6); }
        }

        public void Start(ProgressNotification notification, dynamic options)
        {
            _attemptedSeries = new List<int>();
            ScanSeries(notification);
        }

        private void ScanSeries(ProgressNotification notification)
        {
            var syncList = _seriesProvider.All().Where(s => s.LastInfoSync == null && !_attemptedSeries.Contains(s.SeriesId)).ToList();
            if (syncList.Count == 0)
            {
                return;
            }

            foreach (var currentSeries in syncList)
            {
                try
                {
                    _attemptedSeries.Add(currentSeries.SeriesId);
                    notification.CurrentMessage = String.Format("Searching for '{0}'", new DirectoryInfo(currentSeries.Path).Name);

                    _updateInfoJob.Start(notification, new { SeriesId = currentSeries.SeriesId });
                    _diskScanJob.Start(notification, new { SeriesId = currentSeries.SeriesId });

                    var updatedSeries = _seriesProvider.Get(currentSeries.SeriesId);
                    AutoIgnoreSeasons(updatedSeries.SeriesId);

                    //Download the banner for the new series
                    _bannerDownloadJob.Start(notification, new { SeriesId = updatedSeries.SeriesId });

                    //Get Scene Numbering if applicable
                    _xemUpdateJob.Start(notification, new { SeriesId = updatedSeries.SeriesId });

                    notification.CurrentMessage = String.Format("{0} was successfully imported", updatedSeries.Title);
                }

                catch (Exception e)
                {
                    Logger.ErrorException(e.Message, e);
                }
            }

            //Keep scanning until there no more shows left.
            ScanSeries(notification);
        }

        public void AutoIgnoreSeasons(int seriesId)
        {
            var episodeFiles = _mediaFileProvider.GetSeriesFiles(seriesId);

            if (episodeFiles.Count() != 0)
            {
                var seasons = _episodeProvider.GetSeasons(seriesId);
                var currentSeasons = seasons.Max();

                foreach (var season in seasons)
                {
                    if (season != currentSeasons && !episodeFiles.Any(e => e.SeasonNumber == season))
                    {
                        _seasonProvider.SetIgnore(seriesId, season, true);
                    }
                }
            }
        }
    }
}