using System;
using NLog;
using NzbDrone.Common;

namespace NzbDrone.Core.MediaFiles
{
    public interface ICleanGhostFiles
    {
        void RemoveNonExistingFiles(int seriesId);
    }


    public class GhostFileCleanupService : ICleanGhostFiles
    {
        private readonly IMediaFileService _mediaFileService;
        private readonly DiskProvider _diskProvider;
        private readonly Logger _logger;

        public GhostFileCleanupService(IMediaFileService mediaFileService, DiskProvider diskProvider, Logger logger)
        {
            _mediaFileService = mediaFileService;
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public void RemoveNonExistingFiles(int seriesId)
        {
            var seriesFile = _mediaFileService.GetFilesBySeries(seriesId);

            foreach (var episodeFile in seriesFile)
            {
                try
                {
                    if (!_diskProvider.FileExists(episodeFile.Path))
                    {
                        _logger.Trace("File [{0}] no longer exists on disk. removing from db", episodeFile.Path);
                        _mediaFileService.Delete(episodeFile);
                    }
                }
                catch (Exception ex)
                {
                    var message = String.Format("Unable to cleanup EpisodeFile in DB: {0}", episodeFile.Id);
                    _logger.ErrorException(message, ex);
                }
            }
        }
    }
}