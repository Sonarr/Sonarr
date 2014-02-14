using System.Linq;
using NLog;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Metadata.Files;

namespace NzbDrone.Core.Metadata
{
    public class NotificationService
        : IHandle<MediaCoversUpdatedEvent>,
          IHandle<EpisodeImportedEvent>,
          IHandle<SeriesRenamedEvent>
    {
        private readonly IMetadataFactory _metadataFactory;
        private readonly IMetadataFileService _metadataFileService;
        private readonly Logger _logger;

        public NotificationService(IMetadataFactory metadataFactory, IMetadataFileService metadataFileService, Logger logger)
        {
            _metadataFactory = metadataFactory;
            _metadataFileService = metadataFileService;
            _logger = logger;
        }

        public void Handle(MediaCoversUpdatedEvent message)
        {
            var seriesMetadata = _metadataFileService.GetFilesBySeries(message.Series.Id);

            foreach (var consumer in _metadataFactory.Enabled())
            {
                consumer.OnSeriesUpdated(message.Series, seriesMetadata.Where(c => c.Consumer == consumer.GetType().Name).ToList());
            }
        }

        public void Handle(EpisodeImportedEvent message)
        {
            foreach (var consumer in _metadataFactory.Enabled())
            {
                consumer.OnEpisodeImport(message.EpisodeInfo.Series, message.ImportedEpisode, message.NewDownload);
            }
        }

        public void Handle(SeriesRenamedEvent message)
        {
            foreach (var consumer in _metadataFactory.Enabled())
            {
                consumer.AfterRename(message.Series);
            }
        }
    }
}
