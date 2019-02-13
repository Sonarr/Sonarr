using NLog;
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
        private readonly IMediaFileService _mediaFileService;
        private readonly IConfigService _configService;
        private readonly IUpdateMediaInfo _mediaInfoUpdater;
        private readonly Logger _logger;

        public UpdateMediaInfoService(IMediaFileService mediaFileService,
            IConfigService configService,
            IUpdateMediaInfo mediaInfoUpdater,
            Logger logger)
        {
            _mediaFileService = mediaFileService;
            _configService = configService;
            _mediaInfoUpdater = mediaInfoUpdater;
            _logger = logger;
        }

        private void UpdateMediaInfo(Series series, List<EpisodeFile> mediaFiles)
        {
            foreach (var mediaFile in mediaFiles)
            {
                _mediaInfoUpdater.Update(mediaFile, series);
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
