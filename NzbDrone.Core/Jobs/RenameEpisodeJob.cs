using System.Linq;
using System;
using Ninject;
using NLog;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;

namespace NzbDrone.Core.Jobs
{
    public class RenameEpisodeJob : IJob
    {
        private readonly DiskScanProvider _diskScanProvider;
        private readonly MediaFileProvider _mediaFileProvider;
        private readonly ExternalNotificationProvider _externalNotificationProvider;
        private readonly SeriesProvider _seriesProvider;


        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public RenameEpisodeJob(DiskScanProvider diskScanProvider, MediaFileProvider mediaFileProvider,
                                    ExternalNotificationProvider externalNotificationProvider, SeriesProvider seriesProvider)
        {
            _diskScanProvider = diskScanProvider;
            _mediaFileProvider = mediaFileProvider;
            _externalNotificationProvider = externalNotificationProvider;
            _seriesProvider = seriesProvider;
        }

        public string Name
        {
            get { return "Rename Episode"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromTicks(0); }
        }

        public void Start(ProgressNotification notification, int targetId, int secondaryTargetId)
        {
            if (targetId <= 0)
                throw new ArgumentOutOfRangeException("targetId");

            var episode = _mediaFileProvider.GetEpisodeFile(targetId);
            _diskScanProvider.MoveEpisodeFile(episode);

            //Start AfterRename
            var series = _seriesProvider.GetSeries(episode.SeriesId);
            var message = String.Format("Renamed: Series {0}, Season: {1}", series.Title, secondaryTargetId);
            _externalNotificationProvider.AfterRename(message, series);

            notification.CurrentMessage = String.Format("Episode rename completed for: {0} ", targetId);
        }
    }
}