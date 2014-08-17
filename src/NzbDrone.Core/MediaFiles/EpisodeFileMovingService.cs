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
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public EpisodeFileMovingService(IEpisodeService episodeService,
                                IUpdateEpisodeFileService updateEpisodeFileService,
                                IBuildFileNames buildFileNames,
                                IDiskProvider diskProvider,
                                IConfigService configService,
                                Logger logger)
        {
            _episodeService = episodeService;
            _updateEpisodeFileService = updateEpisodeFileService;
            _buildFileNames = buildFileNames;
            _diskProvider = diskProvider;
            _configService = configService;
            _logger = logger;
        }

        public EpisodeFile MoveEpisodeFile(EpisodeFile episodeFile, Series series)
        {
            var episodes = _episodeService.GetEpisodesByFileId(episodeFile.Id);
            var newFileName = _buildFileNames.BuildFileName(episodes, series, episodeFile);
            var filePath = _buildFileNames.BuildFilePath(series, episodes.First().SeasonNumber, newFileName, Path.GetExtension(episodeFile.RelativePath));

            _logger.Debug("Renaming episode file: {0} to {1}", episodeFile, filePath);
            
            return TransferFile(episodeFile, series, episodes, filePath, TransferMode.Move);
        }

        public EpisodeFile MoveEpisodeFile(EpisodeFile episodeFile, LocalEpisode localEpisode)
        {
            var newFileName = _buildFileNames.BuildFileName(localEpisode.Episodes, localEpisode.Series, episodeFile);
            var filePath = _buildFileNames.BuildFilePath(localEpisode.Series, localEpisode.SeasonNumber, newFileName, Path.GetExtension(localEpisode.Path));

            _logger.Debug("Moving episode file: {0} to {1}", episodeFile, filePath);
            
            return TransferFile(episodeFile, localEpisode.Series, localEpisode.Episodes, filePath, TransferMode.Move);
        }

        public EpisodeFile CopyEpisodeFile(EpisodeFile episodeFile, LocalEpisode localEpisode)
        {
            var newFileName = _buildFileNames.BuildFileName(localEpisode.Episodes, localEpisode.Series, episodeFile);
            var filePath = _buildFileNames.BuildFilePath(localEpisode.Series, localEpisode.SeasonNumber, newFileName, Path.GetExtension(localEpisode.Path));

            _logger.Debug("Copying episode file: {0} to {1}", episodeFile, filePath);

            if (_configService.CopyUsingHardlinks)
            {
                return TransferFile(episodeFile, localEpisode.Series, localEpisode.Episodes, filePath, TransferMode.HardLinkOrCopy);
            }
            else
            {
                return TransferFile(episodeFile, localEpisode.Series, localEpisode.Episodes, filePath, TransferMode.Copy);
            }
        }
        
        private EpisodeFile TransferFile(EpisodeFile episodeFile, Series series, List<Episode> episodes, string destinationFilename, TransferMode mode)
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
                
                SetFolderPermissions(directoryName);

                if (!directoryName.PathEquals(series.Path))
                {
                    SetFolderPermissions(series.Path);
                }
            }

            _logger.Debug("{0} [{1}] > [{2}]", mode, episodeFilePath, destinationFilename);
            _diskProvider.TransferFile(episodeFilePath, destinationFilename, mode);

            episodeFile.RelativePath = series.Path.GetRelativePath(destinationFilename);

            _updateEpisodeFileService.ChangeFileDateForFile(episodeFile, series, episodes);

            try
            {
                SetFolderLastWriteTime(series.Path, episodeFile.DateAdded);

                if (series.SeasonFolder)
                {
                    var seasonFolder = Path.GetDirectoryName(destinationFilename);

                    SetFolderLastWriteTime(seasonFolder, episodeFile.DateAdded);
                }
            }

            catch (Exception ex)
            {
                _logger.WarnException("Unable to set last write time", ex);
            }

            //We should only run this on Windows
            if (OsInfo.IsWindows)
            {
                //Wrapped in Try/Catch to prevent this from causing issues with remote NAS boxes, the move worked, which is more important.
                try
                {
                    _diskProvider.InheritFolderPermissions(destinationFilename);
                }
                catch (Exception ex)
                {
                    if (ex is UnauthorizedAccessException || ex is InvalidOperationException)
                    {
                        _logger.Debug("Unable to apply folder permissions to: ", destinationFilename);
                        _logger.DebugException(ex.Message, ex);
                    }

                    else
                    {
                        throw;
                    }
                }
            }

            else
            {
                SetPermissions(destinationFilename, _configService.FileChmod);
            }

            return episodeFile;
        }

        private void SetPermissions(string path, string permissions)
        {
            if (!_configService.SetPermissionsLinux)
            {
                return;
            }

            try
            {
                _diskProvider.SetPermissions(path, permissions, _configService.ChownUser, _configService.ChownGroup);
            }

            catch (Exception ex)
            {

                _logger.WarnException("Unable to apply permissions to: " + path, ex);
                _logger.DebugException(ex.Message, ex);
            }
        }

        private void SetFolderPermissions(string path)
        {
            SetPermissions(path, _configService.FolderChmod);
        }

        private void SetFolderLastWriteTime(String path, DateTime time)
        {
            if (OsInfo.IsMono) return;

            _logger.Debug("Setting last write time on series folder: {0}", path);
            _diskProvider.FolderSetLastWriteTime(path, time);
        }
    }
}