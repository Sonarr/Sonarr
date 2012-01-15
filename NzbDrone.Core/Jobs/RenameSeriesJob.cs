using System.Linq;
using System;
using NLog;
using Ninject;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;

namespace NzbDrone.Core.Jobs
{
    public class RenameSeriesJob : IJob
    {
        private readonly MediaFileProvider _mediaFileProvider;
        private readonly DiskScanProvider _diskScanProvider;
        private readonly ExternalNotificationProvider _externalNotificationProvider;
        private readonly SeriesProvider _seriesProvider;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public RenameSeriesJob(MediaFileProvider mediaFileProvider, DiskScanProvider diskScanProvider,
                                ExternalNotificationProvider externalNotificationProvider, SeriesProvider seriesProvider)
        {
            _mediaFileProvider = mediaFileProvider;
            _diskScanProvider = diskScanProvider;
            _externalNotificationProvider = externalNotificationProvider;
            _seriesProvider = seriesProvider;
        }

        public string Name
        {
            get { return "Rename Series"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromTicks(0); }
        }

        public void Start(ProgressNotification notification, int targetId, int secondaryTargetId)
        {
            if (targetId <= 0)
                throw new ArgumentOutOfRangeException("targetId");

            Logger.Debug("Getting episodes from database for series: {0}", targetId);
            var episodeFiles = _mediaFileProvider.GetSeriesFiles(targetId);

            if (episodeFiles == null || episodeFiles.Count == 0)
            {
                Logger.Warn("No episodes in database found for series: {0}", targetId);
                return;
            }

            foreach (var episodeFile in episodeFiles)
            {
                _diskScanProvider.MoveEpisodeFile(episodeFile);
            }

            //Start AfterRename
            var series = _seriesProvider.GetSeries(targetId);
            var message = String.Format("Renamed: Series {0}", series.Title);
            _externalNotificationProvider.AfterRename(message, series);

            notification.CurrentMessage = String.Format("Series rename completed for Series: {0}", targetId);
        }
    }
}