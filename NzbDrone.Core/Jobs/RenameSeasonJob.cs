using System.Collections.Generic;
using System.Linq;
using System;
using NLog;
using Ninject;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Jobs
{
    public class RenameSeasonJob : IJob
    {
        private readonly MediaFileProvider _mediaFileProvider;
        private readonly DiskScanProvider _diskScanProvider;
        private readonly ExternalNotificationProvider _externalNotificationProvider;
        private readonly SeriesProvider _seriesProvider;
        private readonly MetadataProvider _metadataProvider;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public RenameSeasonJob(MediaFileProvider mediaFileProvider, DiskScanProvider diskScanProvider,
                                ExternalNotificationProvider externalNotificationProvider, SeriesProvider seriesProvider,
                                MetadataProvider metadataProvider)
        {
            _mediaFileProvider = mediaFileProvider;
            _diskScanProvider = diskScanProvider;
            _externalNotificationProvider = externalNotificationProvider;
            _seriesProvider = seriesProvider;
            _metadataProvider = metadataProvider;
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

            var newEpisodeFiles = new List<EpisodeFile>();
            var oldEpisodeFiles = new List<EpisodeFile>();

            foreach (var episodeFile in episodeFiles)
            {
                try
                {
                    var newFile = _diskScanProvider.MoveEpisodeFile(episodeFile);

                    if (newFile != null)
                    {
                        newEpisodeFiles.Add(newFile);
                        oldEpisodeFiles.Add(episodeFile);
                    }
                }

                catch (Exception e)
                {
                    logger.WarnException("An error has occurred while renaming file", e);
                }
            }

            //Remove & Create Metadata for episode files
            _metadataProvider.RemoveForEpisodeFiles(oldEpisodeFiles);
            _metadataProvider.CreateForEpisodeFiles(newEpisodeFiles);

            //Start AfterRename

            var message = String.Format("Renamed: Series {0}, Season: {1}", series.Title, secondaryTargetId);
            _externalNotificationProvider.AfterRename(message, series);

            notification.CurrentMessage = String.Format("Rename completed for {0} Season {1}", series.Title, secondaryTargetId);
        }
    }
}