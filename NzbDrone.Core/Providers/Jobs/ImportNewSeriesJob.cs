using System;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers.Jobs
{
    /// <summary>
    /// This job processes newly added jobs by downloading their info
    /// from TheTVDB.org and doing a disk scan. this job should only 
    /// </summary>
    public class ImportNewSeriesJob : IJob
    {
        private readonly SeriesProvider _seriesProvider;
        private readonly MediaFileProvider _mediaFileProvider;
        private readonly SeasonProvider _seasonProvider;
        private readonly UpdateInfoJob _updateInfoJob;
        private readonly DiskScanJob _diskScanJob;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ImportNewSeriesJob(SeriesProvider seriesProvider, SeasonProvider seasonProvider,
            MediaFileProvider mediaFileProvider, UpdateInfoJob updateInfoJob, DiskScanJob diskScanJob)
        {
            _seriesProvider = seriesProvider;
            _mediaFileProvider = mediaFileProvider;
            _seasonProvider = seasonProvider;
            _updateInfoJob = updateInfoJob;
            _diskScanJob = diskScanJob;
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
            if (syncList.Count == 0)
            {
                return;
            }

            foreach (var currentSeries in syncList)
            {
                try
                {
                    notification.CurrentMessage = String.Format("Searching for '{0}'", new DirectoryInfo(currentSeries.Path).Name);

                    _updateInfoJob.Start(notification, currentSeries.SeriesId);
                    _diskScanJob.Start(notification, currentSeries.SeriesId);

                    var updatedSeries = _seriesProvider.GetSeries(currentSeries.SeriesId);
                    AutoIgnoreSeasons(updatedSeries);

                }
                catch (Exception e)
                {
                    Logger.ErrorException(e.Message, e);
                }
            }

            //Keep scanning until there no more shows left.
            ScanSeries(notification);
        }

        private void AutoIgnoreSeasons(Series updatedSeries)
        {
            if (_mediaFileProvider.GetSeriesFiles(updatedSeries.SeriesId).Count() != 0)
            {
                Logger.Debug("Looking for seasons to ignore");
                foreach (var season in updatedSeries.Seasons)
                {
                    if (season.SeasonNumber != updatedSeries.Seasons.Max(s => s.SeasonNumber) && _mediaFileProvider.GetSeasonFiles(season.SeasonId).Count() == 0)
                    {
                        Logger.Info("Season {0} of {1} doesn't have any files on disk. season will not be monitored.", season.SeasonNumber, updatedSeries.Title);
                        season.Monitored = false;
                        _seasonProvider.SaveSeason(season);
                    }
                }
            }
        }
    }
}