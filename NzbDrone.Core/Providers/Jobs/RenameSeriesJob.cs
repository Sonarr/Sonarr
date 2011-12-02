using System;
using NLog;
using Ninject;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Model.Notification;

namespace NzbDrone.Core.Providers.Jobs
{
    public class RenameSeriesJob : IJob
    {
        private readonly MediaFileProvider _mediaFileProvider;
        private readonly DiskScanProvider _diskScanProvider;


        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public RenameSeriesJob(MediaFileProvider mediaFileProvider, DiskScanProvider diskScanProvider)
        {
            _mediaFileProvider = mediaFileProvider;
            _diskScanProvider = diskScanProvider;
        }

        public string Name
        {
            get { return "Rename Series"; }
        }

        public int DefaultInterval
        {
            get { return 0; }
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

            notification.CurrentMessage = String.Format("Series rename completed for Series: {0}", targetId);
        }
    }
}