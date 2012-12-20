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

        public void Start(ProgressNotification notification, dynamic options)
        {
            if (options == null || options.SeriesId <= 0)
                throw new ArgumentException("options");

            if (options.SeasonNumber < 0)
                throw new ArgumentException("options.SeasonNumber");

            var series = _seriesProvider.GetSeries(options.SeriesId);

            notification.CurrentMessage = String.Format("Renaming episodes for {0} Season {1}", series.Title, options.SeasonNumber);

            logger.Debug("Getting episodes from database for series: {0} and season: {1}", options.SeriesId, options.SeasonNumber);
            IList<EpisodeFile> episodeFiles = _mediaFileProvider.GetSeasonFiles(options.SeriesId, options.SeasonNumber);

            if (episodeFiles == null || !episodeFiles.Any())
            {
                logger.Warn("No episodes in database found for series: {0} and season: {1}.", options.SeriesId, options.SeasonNumber);
                return;
            }

            var newEpisodeFiles = new List<EpisodeFile>();
            var oldEpisodeFiles = new List<EpisodeFile>();

            foreach (var episodeFile in episodeFiles)
            {
                try
                {
                    var oldFile = new EpisodeFile(episodeFile);
                    var newFile = _diskScanProvider.MoveEpisodeFile(episodeFile);

                    if (newFile != null)
                    {
                        newEpisodeFiles.Add(newFile);
                        oldEpisodeFiles.Add(oldFile);
                    }
                }

                catch (Exception e)
                {
                    logger.WarnException("An error has occurred while renaming file", e);
                }
            }

            if(!oldEpisodeFiles.Any())
            {
                logger.Trace("No episodes were renamed for: {0} Season {1}, no changes were made", series.Title,
                             options.SeasonNumber);
                notification.CurrentMessage = String.Format("Rename completed for: {0} Season {1}, no changes were made", series.Title, options.SeasonNumber);
                return;
            }

            //Remove & Create Metadata for episode files
            //Todo: Add a metadata manager to avoid this hack
            _metadataProvider.RemoveForEpisodeFiles(oldEpisodeFiles);
            _metadataProvider.CreateForEpisodeFiles(newEpisodeFiles);

            //Start AfterRename
            var message = String.Format("Renamed: Series {0}, Season: {1}", series.Title, options.SeasonNumber);
            _externalNotificationProvider.AfterRename(message, series);

            notification.CurrentMessage = String.Format("Rename completed for {0} Season {1}", series.Title, options.SeasonNumber);
        }
    }
}