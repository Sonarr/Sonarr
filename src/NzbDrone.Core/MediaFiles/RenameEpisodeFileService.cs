using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Instrumentation;
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
                                            IExecute<RenameFilesCommand>
    {
        private readonly ISeriesService _seriesService;
        private readonly IMediaFileService _mediaFileService;
        private readonly IMoveEpisodeFiles _episodeFileMover;
        private readonly IEventAggregator _eventAggregator;
        private readonly IEpisodeService _episodeService;
        private readonly IBuildFileNames _filenameBuilder;
        private readonly Logger _logger;

        public RenameEpisodeFileService(ISeriesService seriesService,
                                        IMediaFileService mediaFileService,
                                        IMoveEpisodeFiles episodeFileMover,
                                        IEventAggregator eventAggregator,
                                        IEpisodeService episodeService,
                                        IBuildFileNames filenameBuilder,
                                        Logger logger)
        {
            _seriesService = seriesService;
            _mediaFileService = mediaFileService;
            _episodeFileMover = episodeFileMover;
            _eventAggregator = eventAggregator;
            _episodeService = episodeService;
            _filenameBuilder = filenameBuilder;
            _logger = logger;
        }

        public List<RenameEpisodeFilePreview> GetRenamePreviews(int seriesId)
        {
            var series = _seriesService.GetSeries(seriesId);
            var episodes = _episodeService.GetEpisodeBySeries(seriesId);
            var files = _mediaFileService.GetFilesBySeries(seriesId);

            return GetPreviews(series, episodes, files)
                .OrderBy(e => e.SeasonNumber)
                .ThenBy(e => e.EpisodeNumbers.First())
                .ToList();
        }

        public List<RenameEpisodeFilePreview> GetRenamePreviews(int seriesId, int seasonNumber)
        {
            var series = _seriesService.GetSeries(seriesId);
            var episodes = _episodeService.GetEpisodesBySeason(seriesId, seasonNumber);
            var files = _mediaFileService.GetFilesBySeason(seriesId, seasonNumber);

            return GetPreviews(series, episodes, files)
                .OrderBy(e => e.EpisodeNumbers.First()).ToList();
        }

        private IEnumerable<RenameEpisodeFilePreview> GetPreviews(Series series, List<Episode> episodes, List<EpisodeFile> files)
        {
            foreach (var file in files)
            {
                var episodesInFile = episodes.Where(e => e.EpisodeFileId == file.Id).ToList();
                var seasonNumber = episodesInFile.First().SeasonNumber;
                var newName = _filenameBuilder.BuildFilename(episodesInFile, series, file);
                var newPath = _filenameBuilder.BuildFilePath(series, seasonNumber, newName, Path.GetExtension(file.Path));

                if (!file.Path.PathEquals(newPath))
                {
                    yield return new RenameEpisodeFilePreview
                                 {
                                     SeriesId = series.Id,
                                     SeasonNumber = seasonNumber,
                                     EpisodeNumbers = episodesInFile.Select(e => e.EpisodeNumber).ToList(),
                                     EpisodeFileId = file.Id,
                                     ExistingPath = GetRelativePath(series.Path, file.Path),
                                     NewPath = GetRelativePath(series.Path, newPath)
                                 };
                }
            }
        }

        private string GetRelativePath(string seriesPath, string path)
        {
            return path.Substring(seriesPath.Length + 1);
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
                _eventAggregator.PublishEvent(new SeriesRenamedEvent(series));
            }
        }

        public void Execute(RenameFilesCommand message)
        {
            var series = _seriesService.GetSeries(message.SeriesId);
            var episodeFiles = _mediaFileService.Get(message.Files);

            _logger.ProgressInfo("Renaming {0} files for {1}", episodeFiles.Count, series.Title);
            RenameFiles(episodeFiles, series);
            _logger.ProgressInfo("Selected Episode Files renamed for {0}", series.Title);
        }
    }
}
