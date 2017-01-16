using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles
{
    public interface IRenameEpisodeFileService
    {
        List<RenameEpisodeFilePreview> GetRenamePreviews(int seriesId);
        List<RenameEpisodeFilePreview> GetRenamePreviews(int seriesId, int seasonNumber);
    }

    public class RenameEpisodeFileService : IRenameEpisodeFileService,
                                            IExecute<RenameFilesCommand>,
                                            IExecute<RenameSeriesCommand>
    {
        private readonly ISeriesService _seriesService;
        private readonly IMediaFileService _mediaFileService;
        private readonly IMoveEpisodeFiles _episodeFileMover;
        private readonly IEventAggregator _eventAggregator;
        private readonly IEpisodeService _episodeService;
        private readonly IBuildFileNames _filenameBuilder;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public RenameEpisodeFileService(ISeriesService seriesService,
                                        IMediaFileService mediaFileService,
                                        IMoveEpisodeFiles episodeFileMover,
                                        IEventAggregator eventAggregator,
                                        IEpisodeService episodeService,
                                        IBuildFileNames filenameBuilder,
                                        IDiskProvider diskProvider,
                                        Logger logger)
        {
            _seriesService = seriesService;
            _mediaFileService = mediaFileService;
            _episodeFileMover = episodeFileMover;
            _eventAggregator = eventAggregator;
            _episodeService = episodeService;
            _filenameBuilder = filenameBuilder;
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public List<RenameEpisodeFilePreview> GetRenamePreviews(int seriesId)
        {
            var series = _seriesService.GetSeries(seriesId);
            var episodes = _episodeService.GetEpisodeBySeries(seriesId);
            var files = _mediaFileService.GetFilesBySeries(seriesId);

            return GetPreviews(series, episodes, files)
                .OrderByDescending(e => e.SeasonNumber)
                .ThenByDescending(e => e.EpisodeNumbers.First())
                .ToList();
        }

        public List<RenameEpisodeFilePreview> GetRenamePreviews(int seriesId, int seasonNumber)
        {
            var series = _seriesService.GetSeries(seriesId);
            var episodes = _episodeService.GetEpisodesBySeason(seriesId, seasonNumber);
            var files = _mediaFileService.GetFilesBySeason(seriesId, seasonNumber);

            return GetPreviews(series, episodes, files)
                .OrderByDescending(e => e.EpisodeNumbers.First()).ToList();
        }

        private IEnumerable<RenameEpisodeFilePreview> GetPreviews(Series series, List<Episode> episodes, List<EpisodeFile> files)
        {
            foreach (var f in files)
            {
                var file = f;
                var episodesInFile = episodes.Where(e => e.EpisodeFileId == file.Id).ToList();
                var episodeFilePath = Path.Combine(series.Path, file.RelativePath);

                if (!episodesInFile.Any())
                {
                    _logger.Warn("File ({0}) is not linked to any episodes", episodeFilePath);
                    continue;
                }

                var seasonNumber = episodesInFile.First().SeasonNumber;
                var newName = _filenameBuilder.BuildFileName(episodesInFile, series, file);
                var newPath = _filenameBuilder.BuildFilePath(series, seasonNumber, newName, Path.GetExtension(episodeFilePath));

                if (!episodeFilePath.PathEquals(newPath, StringComparison.Ordinal))
                {
                    yield return new RenameEpisodeFilePreview
                    {
                        SeriesId = series.Id,
                        SeasonNumber = seasonNumber,
                        EpisodeNumbers = episodesInFile.Select(e => e.EpisodeNumber).ToList(),
                        EpisodeFileId = file.Id,
                        ExistingPath = file.RelativePath,
                        NewPath = series.Path.GetRelativePath(newPath)
                    };
                }
            }
        }

        private void RenameFiles(List<EpisodeFile> episodeFiles, Series series)
        {
            var renamed = new List<EpisodeFile>();

            foreach (var episodeFile in episodeFiles)
            {
                var episodeFilePath = Path.Combine(series.Path, episodeFile.RelativePath);

                try
                {
                    _logger.Debug("Renaming episode file: {0}", episodeFile);
                    _episodeFileMover.MoveEpisodeFile(episodeFile, series);

                    _mediaFileService.Update(episodeFile);
                    renamed.Add(episodeFile);

                    _logger.Debug("Renamed episode file: {0}", episodeFile);
                }
                catch (SameFilenameException ex)
                {
                    _logger.Debug("File not renamed, source and destination are the same: {0}", ex.Filename);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to rename file {0}", episodeFilePath);
                }
            }

            if (renamed.Any())
            {
                _diskProvider.RemoveEmptySubfolders(series.Path);

                _eventAggregator.PublishEvent(new SeriesRenamedEvent(series));
            }
        }

        public void Execute(RenameFilesCommand message)
        {
            var series = _seriesService.GetSeries(message.SeriesId);
            var episodeFiles = _mediaFileService.Get(message.Files);

            _logger.ProgressInfo("Renaming {0} files for {1}", episodeFiles.Count, series.Title);
            RenameFiles(episodeFiles, series);
            _logger.ProgressInfo("Selected episode files renamed for {0}", series.Title);
        }

        public void Execute(RenameSeriesCommand message)
        {
            _logger.Debug("Renaming all files for selected series");
            var seriesToRename = _seriesService.GetSeries(message.SeriesIds);

            foreach (var series in seriesToRename)
            {
                var episodeFiles = _mediaFileService.GetFilesBySeries(series.Id);
                _logger.ProgressInfo("Renaming all files in series: {0}", series.Title);
                RenameFiles(episodeFiles, series);
                _logger.ProgressInfo("All episode files renamed for {0}", series.Title);
            }
        }
    }
}
