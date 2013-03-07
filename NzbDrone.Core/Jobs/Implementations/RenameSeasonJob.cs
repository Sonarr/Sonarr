using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Eventing;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Jobs.Implementations
{
    public class RenameSeasonJob : IJob
    {
        private readonly IMediaFileService _mediaFileService;
        private readonly ISeriesRepository _seriesRepository;
        private readonly IEventAggregator _eventAggregator;
        private readonly IMoveEpisodeFiles _episodeFilesMover;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public RenameSeasonJob(IMediaFileService mediaFileService, ISeriesRepository seriesRepository, IEventAggregator eventAggregator, IMoveEpisodeFiles episodeFilesMover)
        {
            _mediaFileService = mediaFileService;
            _seriesRepository = seriesRepository;
            _eventAggregator = eventAggregator;
            _episodeFilesMover = episodeFilesMover;
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

            var series = _seriesRepository.Get((int)options.SeriesId);

            notification.CurrentMessage = String.Format("Renaming episodes for {0} Season {1}", series.Title, options.SeasonNumber);

            logger.Debug("Getting episodes from database for series: {0} and season: {1}", options.SeriesId, options.SeasonNumber);
            IList<EpisodeFile> episodeFiles = _mediaFileService.GetFilesBySeason((int)options.SeriesId, (int)options.SeasonNumber);

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
                    var newFile = _episodeFilesMover.MoveEpisodeFile(episodeFile);

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

            if (!oldEpisodeFiles.Any())
            {
                logger.Trace("No episodes were renamed for: {0} Season {1}, no changes were made", series.Title,
                             options.SeasonNumber);
                notification.CurrentMessage = String.Format("Rename completed for: {0} Season {1}, no changes were made", series.Title, options.SeasonNumber);
                return;
            }

            //Start AfterRename
            _eventAggregator.Publish(new SeriesRenamedEvent(series));

            notification.CurrentMessage = String.Format("Rename completed for {0} Season {1}", series.Title, options.SeasonNumber);
        }
    }
}