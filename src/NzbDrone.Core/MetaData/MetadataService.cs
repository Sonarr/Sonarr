using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Metadata.Files;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Metadata
{
    public class MetadataService
        : IHandle<MediaCoversUpdatedEvent>,
          IHandle<EpisodeImportedEvent>,
          IHandle<SeriesRenamedEvent>
    {
        private readonly IMetadataFactory _metadataFactory;
        private readonly IMetadataFileService _metadataFileService;
        private readonly ICleanMetadataService _cleanMetadataService;
        private readonly IMediaFileService _mediaFileService;
        private readonly IEpisodeService _episodeService;
        private readonly Logger _logger;

        public MetadataService(IMetadataFactory metadataFactory,
                               IMetadataFileService metadataFileService,
                               ICleanMetadataService cleanMetadataService,
                               IMediaFileService mediaFileService,
                               IEpisodeService episodeService,
                               Logger logger)
        {
            _metadataFactory = metadataFactory;
            _metadataFileService = metadataFileService;
            _cleanMetadataService = cleanMetadataService;
            _mediaFileService = mediaFileService;
            _episodeService = episodeService;
            _logger = logger;
        }

        public void Handle(MediaCoversUpdatedEvent message)
        {
            _cleanMetadataService.Clean(message.Series);
            var seriesMetadata = _metadataFileService.GetFilesBySeries(message.Series.Id);

            foreach (var consumer in _metadataFactory.Enabled())
            {
                consumer.OnSeriesUpdated(message.Series, GetMetadataFilesForConsumer(consumer, seriesMetadata), GetEpisodeFiles(message.Series.Id));
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
            var seriesMetadata = _metadataFileService.GetFilesBySeries(message.Series.Id);

            foreach (var consumer in _metadataFactory.Enabled())
            {
                consumer.AfterRename(message.Series, GetMetadataFilesForConsumer(consumer, seriesMetadata), GetEpisodeFiles(message.Series.Id));
            }
        }

        private List<EpisodeFile> GetEpisodeFiles(int seriesId)
        {
            var episodeFiles = _mediaFileService.GetFilesBySeries(seriesId);
            var episodes = _episodeService.GetEpisodeBySeries(seriesId);

            foreach (var episodeFile in episodeFiles)
            {
                var localEpisodeFile = episodeFile;
                episodeFile.Episodes = new LazyList<Episode>(episodes.Where(e => e.EpisodeFileId == localEpisodeFile.Id));
            }

            return episodeFiles;
        }

        private List<MetadataFile> GetMetadataFilesForConsumer(IMetadata consumer, List<MetadataFile> seriesMetadata)
        {
            return seriesMetadata.Where(c => c.Consumer == consumer.GetType().Name).ToList();
        }
    }
}
