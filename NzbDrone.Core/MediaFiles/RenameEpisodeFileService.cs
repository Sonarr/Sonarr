using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging;
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

            foreach (var episodeFile in episodeFiles)
            {
                try
                {
                    _logger.Trace("Renaming episode file: {0}", episodeFile);
                    episodeFile.Path = _episodeFileMover.MoveEpisodeFile(episodeFile, series);

                    _mediaFileService.Update(episodeFile);
                    renamed.Add(episodeFile);

                    _logger.Trace("Renamed episode file: {0}", episodeFile);
                }
                catch (SameFilenameException ex)
                {
                    _logger.Trace("File not renamed, source and destination are the same: {0}", ex.Filename);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Failed to rename file: " + episodeFile.Path, ex);
                }
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

            _logger.ProgressInfo("Renaming {0} files for {1} season {2}", episodeFiles.Count, series.Title, message.SeasonNumber);
            RenameFiles(episodeFiles, series);
            _logger.ProgressInfo("Episode Fies renamed for {0} season {1}", series.Title, message.SeasonNumber);
        }

        public void Execute(RenameSeriesCommand message)
        {
            var series = _seriesService.GetSeries(message.SeriesId);
            var episodeFiles = _mediaFileService.GetFilesBySeries(message.SeriesId);

            _logger.ProgressInfo("Renaming {0} files for {1}", episodeFiles.Count, series.Title);
            RenameFiles(episodeFiles, series);
            _logger.ProgressInfo("Episode Fies renamed for {0}", series.Title);
        }
    }
}
