using System;
using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles.MediaInfo
{
    public interface IUpdateMediaInfo
    {
        void Update(EpisodeFile mediaFile, Series series);
    }

    public class MediaInfoUpdater : IUpdateMediaInfo
    {
        private readonly IVideoFileInfoReader _videoFileInfoReader;
        private readonly IDiskProvider _diskProvider;
        private readonly IMediaFileService _mediaFileService;
        private readonly Logger _logger;

        public MediaInfoUpdater(Logger logger, IMediaFileService mediaFileService, IDiskProvider diskProvider, IVideoFileInfoReader videoFileInfoReader)
        {
            _logger = logger;
            _mediaFileService = mediaFileService;
            _diskProvider = diskProvider;
            _videoFileInfoReader = videoFileInfoReader;
        }

        public void Update(EpisodeFile mediaFile, Series series)
        {
            if (string.IsNullOrEmpty(series.Path) || string.IsNullOrEmpty(mediaFile.RelativePath)) return;

            var path = Path.Combine(series.Path, mediaFile.RelativePath);
            Update(mediaFile, path);
        }

        private void Update(EpisodeFile mediaFile, string path)
        {
            if (string.IsNullOrEmpty(path) || !_diskProvider.FileExists(path))
            {
                _logger.Debug("Can't update MediaInfo because '{0}' does not exist", path);
                return;
            }

            mediaFile.MediaInfo = _videoFileInfoReader.GetMediaInfo(path);

            if (mediaFile.MediaInfo == null) return;

            _mediaFileService.Update(mediaFile);
            _logger.Debug("Updated MediaInfo for '{0}'", path);
        }
    }
}