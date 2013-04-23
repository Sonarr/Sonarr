using System;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Eventing;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles
{
    public interface IMoveEpisodeFiles
    {
        EpisodeFile MoveEpisodeFile(EpisodeFile episodeFile, bool newDownload = false);
    }

    public class MoveEpisodeFiles : IMoveEpisodeFiles
    {
        private readonly ISeriesRepository _seriesRepository;
        private readonly IEpisodeService _episodeService;
        private readonly IBuildFileNames _buildFileNames;
        private readonly IMediaFileService _mediaFileService;
        private readonly IEventAggregator _eventAggregator;
        private readonly DiskProvider _diskProvider;
        private readonly Logger _logger;

        public MoveEpisodeFiles(ISeriesRepository seriesRepository, IEpisodeService episodeService, IBuildFileNames buildFileNames, IMediaFileService mediaFileService, IEventAggregator eventAggregator, DiskProvider diskProvider, Logger logger)
        {
            _seriesRepository = seriesRepository;
            _episodeService = episodeService;
            _buildFileNames = buildFileNames;
            _mediaFileService = mediaFileService;
            _eventAggregator = eventAggregator;
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public EpisodeFile MoveEpisodeFile(EpisodeFile episodeFile, bool newDownload = false)
        {
            if (episodeFile == null)
                throw new ArgumentNullException("episodeFile");

            var series = _seriesRepository.Get(episodeFile.SeriesId);
            var episodes = _episodeService.GetEpisodesByFileId(episodeFile.Id);
            string newFileName = _buildFileNames.BuildFilename(episodes, series, episodeFile);
            var newFile = _buildFileNames.BuildFilePath(series, episodes.First().SeasonNumber, newFileName, Path.GetExtension(episodeFile.Path));

            //Only rename if existing and new filenames don't match
            if (DiskProvider.PathEquals(episodeFile.Path, newFile))
            {
                _logger.Debug("Skipping file rename, source and destination are the same: {0}", episodeFile.Path);
                return null;
            }

            if (!_diskProvider.FileExists(episodeFile.Path))
            {
                _logger.Error("Episode file path does not exist, {0}", episodeFile.Path);
                return null;
            }

            _diskProvider.CreateFolder(new FileInfo(newFile).DirectoryName);

            _logger.Debug("Moving [{0}] > [{1}]", episodeFile.Path, newFile);
            _diskProvider.MoveFile(episodeFile.Path, newFile);

            //Wrapped in Try/Catch to prevent this from causing issues with remote NAS boxes, the move worked, which is more important.
            try
            {
                _diskProvider.InheritFolderPermissions(newFile);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.Debug("Unable to apply folder permissions to: ", newFile);
                _logger.TraceException(ex.Message, ex);
            }

            episodeFile.Path = newFile;
            _mediaFileService.Update(episodeFile);

            var parsedEpisodeInfo = Parser.Parser.ParsePath(episodeFile.Path);
            parsedEpisodeInfo.Quality = episodeFile.Quality;

            if (newDownload)
            {
                _eventAggregator.Publish(new EpisodeDownloadedEvent(parsedEpisodeInfo, series));
            }

            return episodeFile;
        }
    }
}