using System.Linq;
using System;
using NLog;
using Ninject;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;

namespace NzbDrone.Core.Jobs
{
    public class RenameSeasonJob : IJob
    {
        private readonly MediaFileProvider _mediaFileProvider;
        private readonly DiskScanProvider _diskScanProvider;
        private readonly ExternalNotificationProvider _externalNotificationProvider;
        private readonly SeriesProvider _seriesProvider;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public RenameSeasonJob(MediaFileProvider mediaFileProvider, DiskScanProvider diskScanProvider,
                                ExternalNotificationProvider externalNotificationProvider, SeriesProvider seriesProvider)
        {
            _mediaFileProvider = mediaFileProvider;
            _diskScanProvider = diskScanProvider;
            _externalNotificationProvider = externalNotificationProvider;
            _seriesProvider = seriesProvider;
        }

        public string Name
        {
            get { return "Rename Season"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromTicks(0); }
        }

        public void Start(ProgressNotification notification, int targetId, int secondaryTargetId)
        {
            if (targetId <= 0)
                throw new ArgumentOutOfRangeException("targetId");

            if (secondaryTargetId <= 0)
                throw new ArgumentOutOfRangeException("secondaryTargetId");

            var series = _seriesProvider.GetSeries(targetId);

            notification.CurrentMessage = String.Format("Renaming episodes for {0} Season {1}", series.Title, secondaryTargetId);

            logger.Debug("Getting episodes from database for series: {0} and season: {1}", targetId, secondaryTargetId);
            var episodeFiles = _mediaFileProvider.GetSeasonFiles(targetId, secondaryTargetId);

            if (episodeFiles == null || !episodeFiles.Any())
            {
                logger.Warn("No episodes in database found for series: {0} and season: {1}.", targetId, secondaryTargetId);
                return;
            }

            foreach (var episodeFile in episodeFiles)
            {
                try
                {
                    _diskScanProvider.MoveEpisodeFile(episodeFile);
                }
                catch (Exception exception)
                {
                    logger.WarnException("An error has occurred while renaming file", exception);
                }
            }

            //Start AfterRename

            var message = String.Format("Renamed: Series {0}, Season: {1}", series.Title, secondaryTargetId);
            _externalNotificationProvider.AfterRename(message, series);

            notification.CurrentMessage = String.Format("Rename completed for {0} Season {1}", series.Title, secondaryTargetId);
        }
    }
}