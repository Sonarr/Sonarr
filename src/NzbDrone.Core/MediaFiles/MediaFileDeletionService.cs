using System;
using System.IO;
using System.Net;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.MediaFiles
{
    public interface IDeleteMediaFiles
    {
        void DeleteEpisodeFile(Series series, EpisodeFile episodeFile);
    }

    public class MediaFileDeletionService : IDeleteMediaFiles, IHandleAsync<SeriesDeletedEvent>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IRecycleBinProvider _recycleBinProvider;
        private readonly IMediaFileService _mediaFileService;
        private readonly Logger _logger;

        public MediaFileDeletionService(IDiskProvider diskProvider,
                                        IRecycleBinProvider recycleBinProvider,
                                        IMediaFileService mediaFileService,
                                        Logger logger)
        {
            _diskProvider = diskProvider;
            _recycleBinProvider = recycleBinProvider;
            _mediaFileService = mediaFileService;
            _logger = logger;
        }

        public void DeleteEpisodeFile(Series series, EpisodeFile episodeFile)
        {
            var fullPath = Path.Combine(series.Path, episodeFile.RelativePath);
            var rootFolder = _diskProvider.GetParentFolder(series.Path);

            if (!_diskProvider.FolderExists(rootFolder))
            {
                throw new NzbDroneClientException(HttpStatusCode.Conflict, "Series' root folder ({0}) doesn't exist.", rootFolder);
            }

            if (_diskProvider.GetDirectories(rootFolder).Empty())
            {
                throw new NzbDroneClientException(HttpStatusCode.Conflict, "Series' root folder ({0}) is empty.", rootFolder);
            }

            if (_diskProvider.FolderExists(series.Path) && _diskProvider.FileExists(fullPath))
            {
                _logger.Info("Deleting episode file: {0}", fullPath);

                var subfolder = _diskProvider.GetParentFolder(series.Path).GetRelativePath(_diskProvider.GetParentFolder(fullPath));

                try
                {
                    _recycleBinProvider.DeleteFile(fullPath, subfolder);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Unable to delete episode file");
                    throw new NzbDroneClientException(HttpStatusCode.InternalServerError, "Unable to delete episode file");
                }
            }
            
            // Delete the episode file from the database to clean it up even if the file was already deleted
            _mediaFileService.Delete(episodeFile, DeleteMediaFileReason.Manual);
        }

        public void HandleAsync(SeriesDeletedEvent message)
        {
            if (message.DeleteFiles)
            {
                if (_diskProvider.FolderExists(message.Series.Path))
                {
                    _recycleBinProvider.DeleteFolder(message.Series.Path);
                }
            }
        }
    }
}
