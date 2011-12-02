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


        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public RenameSeasonJob(MediaFileProvider mediaFileProvider, DiskScanProvider diskScanProvider)
        {
            _mediaFileProvider = mediaFileProvider;
            _diskScanProvider = diskScanProvider;
        }

        public string Name
        {
            get { return "Rename Season"; }
        }

        public int DefaultInterval
        {
            get { return 0; }
        }

        public void Start(ProgressNotification notification, int targetId, int secondaryTargetId)
        {
            if (targetId <= 0)
                throw new ArgumentOutOfRangeException("targetId");

            if (secondaryTargetId <= 0)
                throw new ArgumentOutOfRangeException("secondaryTargetId");

            Logger.Debug("Getting episodes from database for series: {0} and season: {1}", targetId, secondaryTargetId);
            var episodeFiles = _mediaFileProvider.GetSeasonFiles(targetId, secondaryTargetId);

            if (episodeFiles == null || episodeFiles.Count == 0)
            {
                Logger.Warn("No episodes in database found for series: {0} and season: {1}.", targetId, secondaryTargetId);
                return;
            }

            foreach (var episodeFile in episodeFiles)
            {
                _diskScanProvider.MoveEpisodeFile(episodeFile);
            }

            notification.CurrentMessage = String.Format("Season rename completed for Series: {0} Season: {1}", targetId, secondaryTargetId);
        }
    }
}