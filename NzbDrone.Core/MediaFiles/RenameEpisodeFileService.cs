using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles
{
    public class RenameEpisodeFileService : IExecute<RenameSeasonCommand>, IExecute<RenameSeriesCommand>
    {
        private readonly ISeriesService _seriesService;
        private readonly IMediaFileService _mediaFileService;
        private readonly IMoveEpisodeFiles _episodeFileMover;
        private readonly IMessageAggregator _messageAggregator;
        private readonly Logger _logger;

        public RenameEpisodeFileService(ISeriesService seriesService,
                                        IMediaFileService mediaFileService,
                                        IMoveEpisodeFiles episodeFileMover,
                                        IMessageAggregator messageAggregator,
                                        Logger logger)
        {
            _seriesService = seriesService;
            _mediaFileService = mediaFileService;
            _episodeFileMover = episodeFileMover;
            _messageAggregator = messageAggregator;
            _logger = logger;
        }

        private void RenameFiles(List<EpisodeFile> episodeFiles, Series series)
        {
            var renamed = new List<EpisodeFile>();

            foreach (var file in episodeFiles)
            {
                var episodeFile = file;

                _logger.Trace("Renaming episode file: {0}", episodeFile);
                episodeFile = _episodeFileMover.MoveEpisodeFile(episodeFile, series);

                if (episodeFile != null)
                {
                    _mediaFileService.Update(episodeFile);
                    renamed.Add(episodeFile);
                }

                _logger.Trace("Renamed episode file: {0}", episodeFile);
            }

            if (renamed.Any())
            {
                _messageAggregator.PublishEvent(new SeriesRenamedEvent(series));
            }
        }

        public void Execute(RenameSeasonCommand message)
        {
            var series = _seriesService.GetSeries(message.SeriesId);
            var episodeFiles = _mediaFileService.GetFilesBySeason(message.SeriesId, message.SeasonNumber);

            _logger.Info("Renaming {0} files for {1} season {2}", episodeFiles.Count, series.Title, message.SeasonNumber);
            RenameFiles(episodeFiles, series);
            _logger.Debug("Episode Fies renamed for {0} season {1}", series.Title, message.SeasonNumber);
        }

        public void Execute(RenameSeriesCommand message)
        {
            var series = _seriesService.GetSeries(message.SeriesId);
            var episodeFiles = _mediaFileService.GetFilesBySeries(message.SeriesId);

            _logger.Info("Renaming {0} files for {1}", episodeFiles.Count, series.Title);
            RenameFiles(episodeFiles, series);
            _logger.Debug("Episode Fies renamed for {0}", series.Title);
        }
    }
}
