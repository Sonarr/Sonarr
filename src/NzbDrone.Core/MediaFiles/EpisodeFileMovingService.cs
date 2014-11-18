using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles
{
    public interface IMoveEpisodeFiles
    {
        EpisodeFile MoveEpisodeFile(EpisodeFile episodeFile, Series series);
        EpisodeFile MoveEpisodeFile(EpisodeFile episodeFile, LocalEpisode localEpisode);
        EpisodeFile CopyEpisodeFile(EpisodeFile episodeFile, LocalEpisode localEpisode);
    }

    public class EpisodeFileMovingService : IMoveEpisodeFiles
    {
        private readonly IEpisodeService _episodeService;
        private readonly IUpdateEpisodeFileService _updateEpisodeFileService;
        private readonly IBuildFileNames _buildFileNames;
        private readonly IDiskProvider _diskProvider;
        private readonly IMediaFileAttributeService _mediaFileAttributeService;
        private readonly Logger _logger;

        public EpisodeFileMovingService(IEpisodeService episodeService,
                                IUpdateEpisodeFileService updateEpisodeFileService,
                                IBuildFileNames buildFileNames,
                                IDiskProvider diskProvider,
                                IMediaFileAttributeService mediaFileAttributeService,
                                Logger logger)
        {
            _episodeService = episodeService;
            _updateEpisodeFileService = updateEpisodeFileService;
            _buildFileNames = buildFileNames;
            _diskProvider = diskProvider;
            _mediaFileAttributeService = mediaFileAttributeService;
            _logger = logger;
        }

        public EpisodeFile MoveEpisodeFile(EpisodeFile episodeFile, Series series)
        {
            var episodes = _episodeService.GetEpisodesByFileId(episodeFile.Id);
            var newFileName = _buildFileNames.BuildFileName(episodes, series, episodeFile);
            var filePath = _buildFileNames.BuildFilePath(series, episodes.First().SeasonNumber, newFileName, Path.GetExtension(episodeFile.RelativePath));

            _logger.Debug("Renaming episode file: {0} to {1}", episodeFile, filePath);
            
            return TransferFile(episodeFile, series, episodes, filePath, false);
        }

        public EpisodeFile MoveEpisodeFile(EpisodeFile episodeFile, LocalEpisode localEpisode)
        {
            var newFileName = _buildFileNames.BuildFileName(localEpisode.Episodes, localEpisode.Series, episodeFile);
            var filePath = _buildFileNames.BuildFilePath(localEpisode.Series, localEpisode.SeasonNumber, newFileName, Path.GetExtension(localEpisode.Path));

            _logger.Debug("Moving episode file: {0} to {1}", episodeFile, filePath);
            
            return TransferFile(episodeFile, localEpisode.Series, localEpisode.Episodes, filePath, false);
        }

        public EpisodeFile CopyEpisodeFile(EpisodeFile episodeFile, LocalEpisode localEpisode)
        {
            var newFileName = _buildFileNames.BuildFileName(localEpisode.Episodes, localEpisode.Series, episodeFile);
            var filePath = _buildFileNames.BuildFilePath(localEpisode.Series, localEpisode.SeasonNumber, newFileName, Path.GetExtension(localEpisode.Path));

            _logger.Debug("Copying episode file: {0} to {1}", episodeFile, filePath);

            return TransferFile(episodeFile, localEpisode.Series, localEpisode.Episodes, filePath, true);
        }
        
        private EpisodeFile TransferFile(EpisodeFile episodeFile, Series series, List<Episode> episodes, String destinationFilename, Boolean copyOnly)
        {
            Ensure.That(episodeFile, () => episodeFile).IsNotNull();
            Ensure.That(series,() => series).IsNotNull();
            Ensure.That(destinationFilename, () => destinationFilename).IsValidPath();

            var episodeFilePath = episodeFile.Path ?? Path.Combine(series.Path, episodeFile.RelativePath);

            if (!_diskProvider.FileExists(episodeFilePath))
            {
                throw new FileNotFoundException("Episode file path does not exist", episodeFilePath);
            }

            if (episodeFilePath.PathEquals(destinationFilename))
            {
                throw new SameFilenameException("File not moved, source and destination are the same", episodeFilePath);
            }

            var directoryName = new FileInfo(destinationFilename).DirectoryName;

            if (!_diskProvider.FolderExists(directoryName))
            {
                try
                {
                    _diskProvider.CreateFolder(directoryName);
                }
                catch (IOException ex)
                {
                    _logger.ErrorException("Unable to create directory: " + directoryName, ex);
                }
                
                _mediaFileAttributeService.SetFolderPermissions(directoryName);

                if (!directoryName.PathEquals(series.Path))
                {
                    _mediaFileAttributeService.SetFolderPermissions(series.Path);
                }
            }

            if (copyOnly)
            {
                _logger.Debug("Copying [{0}] > [{1}]", episodeFilePath, destinationFilename);
                _diskProvider.CopyFile(episodeFilePath, destinationFilename);
            }
            else
            {
                _logger.Debug("Moving [{0}] > [{1}]", episodeFilePath, destinationFilename);
                _diskProvider.MoveFile(episodeFilePath, destinationFilename);
            }

            episodeFile.RelativePath = series.Path.GetRelativePath(destinationFilename);

            _updateEpisodeFileService.ChangeFileDateForFile(episodeFile, series, episodes);

            try
            {
                _mediaFileAttributeService.SetFolderLastWriteTime(series.Path, episodeFile.DateAdded);

                if (series.SeasonFolder)
                {
                    var seasonFolder = Path.GetDirectoryName(destinationFilename);

                    _mediaFileAttributeService.SetFolderLastWriteTime(seasonFolder, episodeFile.DateAdded);
                }
            }

            catch (Exception ex)
            {
                _logger.WarnException("Unable to set last write time", ex);
            }

            _mediaFileAttributeService.SetFilePermissions(destinationFilename);

            return episodeFile;
        }
    }
}
