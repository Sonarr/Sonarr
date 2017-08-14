using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.MediaFiles.MediaInfo
{
    public class UpdateMediaInfoService : IHandle<SeriesScannedEvent>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IMediaFileService _mediaFileService;
        private readonly IVideoFileInfoReader _videoFileInfoReader;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public UpdateMediaInfoService(IDiskProvider diskProvider,
                                IMediaFileService mediaFileService,
                                IVideoFileInfoReader videoFileInfoReader,
                                IConfigService configService,
                                Logger logger)
        {
            _diskProvider = diskProvider;
            _mediaFileService = mediaFileService;
            _videoFileInfoReader = videoFileInfoReader;
            _configService = configService;
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
            if (!_configService.EnableMediaInfo)
            {
                _logger.Debug("MediaInfo is disabled");
                return;
            }

            var allMediaFiles = _mediaFileService.GetFilesBySeries(message.Series.Id);
            var filteredMediaFiles = allMediaFiles.Where(c => c.MediaInfo == null || c.MediaInfo.SchemaRevision < VideoFileInfoReader.MINIMUM_MEDIA_INFO_SCHEMA_REVISION).ToList();

            UpdateMediaInfo(message.Series, filteredMediaFiles);
        }
    }
}
