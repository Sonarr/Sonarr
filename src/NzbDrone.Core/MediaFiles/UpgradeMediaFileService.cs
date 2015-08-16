using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles
{
    public interface IUpgradeMediaFiles
    {
        FileMoveResult UpgradeFile(MediaModelBase baseFile, LocalItem localItem, bool copyOnly = false);
    }

    public class UpgradeMediaFileService : IUpgradeMediaFiles
    {
        private readonly IRecycleBinProvider _recycleBinProvider;
        private readonly IMediaFileService _mediaFileService;
        private readonly IMoveFiles _FileMover;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public UpgradeMediaFileService(IRecycleBinProvider recycleBinProvider,
                                       IMediaFileService mediaFileService,
                                       IMoveFiles fileMover,
                                       IDiskProvider diskProvider,
                                       Logger logger)
        {
            _recycleBinProvider = recycleBinProvider;
            _mediaFileService = mediaFileService;
            _FileMover = fileMover;
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public FileMoveResult UpgradeFile(MediaModelBase baseFile, LocalItem localItem, bool copyOnly = false)
        {
            var moveFileResult = new FileMoveResult();
            var existingFiles = localItem.MediaFiles.GroupBy(e => e.Id);


            foreach (var existingFile in existingFiles)
            {
                var file = existingFile.First();
                var itemFilePath = Path.Combine(localItem.Media.Path, file.RelativePath);

                if (_diskProvider.FileExists(itemFilePath))
                {
                    _logger.Debug("Removing existing file: {0}", file);
                    _recycleBinProvider.DeleteFile(itemFilePath);
                }

                moveFileResult.OldFiles.Add(file);
                _mediaFileService.Delete(file, DeleteMediaFileReason.Upgrade);
            }

            if (copyOnly)
            {
                moveFileResult.File = _FileMover.CopyFile(baseFile, localItem);
            }
            else
            {
                moveFileResult.File = _FileMover.MoveFile(baseFile, localItem);
            }

            return moveFileResult;
        }
    }
}
