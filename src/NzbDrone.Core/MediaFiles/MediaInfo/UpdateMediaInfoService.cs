using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.MediaFiles.MediaInfo
{
    public class UpdateMediaInfoService : IHandle<SeriesScannedEvent>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IMediaFileService _mediaFileService;
        private readonly IVideoFileInfoReader _videoFileInfoReader;
        private readonly Logger _logger;

        public UpdateMediaInfoService(IDiskProvider diskProvider,
                                IMediaFileService mediaFileService,
                                IVideoFileInfoReader videoFileInfoReader,
                                Logger logger)
        {
            _diskProvider = diskProvider;
            _mediaFileService = mediaFileService;
            _videoFileInfoReader = videoFileInfoReader;
            _logger = logger;
        }

        private void UpdateMediaInfo(Series series, List<EpisodeFile> mediaFiles)
        {
            foreach (var mediaFile in mediaFiles)
            {
                var path = Path.Combine(series.Path, mediaFile.RelativePath);

                if (!_diskProvider.FileExists(path))
                {
                    _logger.Debug("Can't update MediaInfo because '{0}' does not exist", path);
                    continue;
                }

                mediaFile.MediaInfo = _videoFileInfoReader.GetMediaInfo(path);

                if (mediaFile.MediaInfo != null)
                {
                    _mediaFileService.Update(mediaFile);
                    _logger.Debug("Updated MediaInfo for '{0}'", path);
                }
            }
        }

        public void Handle(SeriesScannedEvent message)
        {
            var mediaFiles = _mediaFileService.GetFilesBySeries(message.Series.Id)
                                              .Where(c => c.MediaInfo == null)
                                              .ToList();

            UpdateMediaInfo(message.Series, mediaFiles);
        }
    }
}
