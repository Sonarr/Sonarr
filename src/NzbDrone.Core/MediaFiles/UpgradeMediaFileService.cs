using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles
{
    public interface IUpgradeMediaFiles
    {
        EpisodeFileMoveResult UpgradeEpisodeFile(EpisodeFile episodeFile, LocalEpisode localEpisode, bool copyOnly = false);
    }

    public class UpgradeMediaFileService : IUpgradeMediaFiles
    {
        private readonly IRecycleBinProvider _recycleBinProvider;
        private readonly IMediaFileService _mediaFileService;
        private readonly IMoveEpisodeFiles _episodeFileMover;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public UpgradeMediaFileService(IRecycleBinProvider recycleBinProvider,
                                       IMediaFileService mediaFileService,
                                       IMoveEpisodeFiles episodeFileMover,
                                       IDiskProvider diskProvider,
                                       Logger logger)
        {
            _recycleBinProvider = recycleBinProvider;
            _mediaFileService = mediaFileService;
            _episodeFileMover = episodeFileMover;
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public EpisodeFileMoveResult UpgradeEpisodeFile(EpisodeFile episodeFile, LocalEpisode localEpisode, bool copyOnly = false)
        {
            var moveFileResult = new EpisodeFileMoveResult();
            var existingFiles = localEpisode.Episodes
                                            .Where(e => e.EpisodeFileId > 0)
                                            .Select(e => e.EpisodeFile.Value)
                                            .Where(e => e != null)
                                            .GroupBy(e => e.Id)
                                            .ToList();

            var rootFolder = _diskProvider.GetParentFolder(localEpisode.Series.Path);

            // If there are existing episode files and the root folder is missing, throw, so the old file isn't left behind during the import process.
            if (existingFiles.Any() && !_diskProvider.FolderExists(rootFolder))
            {
                throw new RootFolderNotFoundException($"Root folder '{rootFolder}' was not found.");
            }

            foreach (var existingFile in existingFiles)
            {
                var file = existingFile.First();
                var episodeFilePath = Path.Combine(localEpisode.Series.Path, file.RelativePath);
                var subfolder = rootFolder.GetRelativePath(_diskProvider.GetParentFolder(episodeFilePath));

                if (_diskProvider.FileExists(episodeFilePath))
                {
                    _logger.Debug("Removing existing episode file: {0}", file);
                    _recycleBinProvider.DeleteFile(episodeFilePath, subfolder);
                }

                moveFileResult.OldFiles.Add(file);
                _mediaFileService.Delete(file, DeleteMediaFileReason.Upgrade);
            }

            if (copyOnly)
            {
                moveFileResult.EpisodeFile = _episodeFileMover.CopyEpisodeFile(episodeFile, localEpisode);
            }
            else
            {
                moveFileResult.EpisodeFile = _episodeFileMover.MoveEpisodeFile(episodeFile, localEpisode);
            }

            return moveFileResult;
        }
    }
}
