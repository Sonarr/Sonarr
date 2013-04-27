using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Jobs.Implementations
{
    public class RenameSeriesJob : IJob
    {
        private readonly IMediaFileService _mediaFileService;
        private readonly ISeriesRepository _seriesRepository;
        private readonly IMessageAggregator _messageAggregator;
        private readonly IMoveEpisodeFiles _moveEpisodeFiles;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public RenameSeriesJob(IMediaFileService mediaFileService, ISeriesRepository seriesRepository, IMessageAggregator messageAggregator, IMoveEpisodeFiles moveEpisodeFiles)
        {
            _mediaFileService = mediaFileService;
            _seriesRepository = seriesRepository;
            _messageAggregator = messageAggregator;
            _moveEpisodeFiles = moveEpisodeFiles;
        }

        public string Name
        {
            get { return "Rename Series"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromTicks(0); }
        }

        public void Start(ProgressNotification notification, dynamic options)
        {
            List<Series> seriesToRename;

            if (options == null || options.SeriesId <= 0)
            {
                seriesToRename = _seriesRepository.All().ToList();
            }

            else
            {
                seriesToRename = new List<Series> { _seriesRepository.Get((int)options.SeriesId) };
            }

            foreach (var series in seriesToRename)
            {
                notification.CurrentMessage = String.Format("Renaming episodes for '{0}'", series.Title);

                Logger.Debug("Getting episodes from database for series: {0}", series.Id);
                var episodeFiles = _mediaFileService.GetFilesBySeries(series.Id);

                if (episodeFiles == null || episodeFiles.Count == 0)
                {
                    Logger.Warn("No episodes in database found for series: {0}", series.Id);
                    return;
                }

                var newEpisodeFiles = new List<EpisodeFile>();
                var oldEpisodeFiles = new List<EpisodeFile>();

                foreach (var episodeFile in episodeFiles)
                {
                    try
                    {
                        var oldFile = new EpisodeFile(episodeFile);
                        var newFile = _moveEpisodeFiles.MoveEpisodeFile(episodeFile);

                        if (newFile != null)
                        {
                            newEpisodeFiles.Add(newFile);
                            oldEpisodeFiles.Add(oldFile);
                        }
                    }

                    catch (Exception e)
                    {
                        Logger.WarnException("An error has occurred while renaming file", e);
                    }
                }

                //Start AfterRename

                _messageAggregator.PublishEvent(new SeriesRenamedEvent(series));

                notification.CurrentMessage = String.Format("Rename completed for {0}", series.Title);
            }
        }
    }
}