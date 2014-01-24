using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Metadata.Files;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Metadata
{
    public class ExistingMetadataService : IHandleAsync<SeriesUpdatedEvent>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IMetadataFileService _metadataFileService;
        private readonly IMediaFileService _mediaFileService;
        private readonly Logger _logger;
        private readonly List<IMetadata> _consumers;

        public ExistingMetadataService(IDiskProvider diskProvider,
                                       IEnumerable<IMetadata> consumers,
                                       IMetadataFileService metadataFileService,
                                       IMediaFileService mediaFileService,
                                       Logger logger)
        {
            _diskProvider = diskProvider;
            _metadataFileService = metadataFileService;
            _mediaFileService = mediaFileService;
            _logger = logger;
            _consumers = consumers.ToList();
        }

        public void HandleAsync(SeriesUpdatedEvent message)
        {
            if (!_diskProvider.FolderExists(message.Series.Path)) return;

            _logger.Trace("Looking for existing metadata in {0}", message.Series.Path);

            var filesOnDisk = _diskProvider.GetFiles(message.Series.Path, SearchOption.AllDirectories);
            var possibleMetadataFiles = filesOnDisk.Where(c => !MediaFileExtensions.Extensions.Contains(Path.GetExtension(c).ToLower())).ToList();
            var filteredFiles = _metadataFileService.FilterExistingFiles(possibleMetadataFiles, message.Series);
            
            foreach (var possibleMetadataFile in filteredFiles)
            {
                foreach (var consumer in _consumers)
                {
                    var metadata = consumer.FindMetadataFile(message.Series, possibleMetadataFile);

                    if (metadata == null) continue;

                    if (metadata.Type == MetadataType.EpisodeImage ||
                        metadata.Type == MetadataType.EpisodeMetadata)
                    {
                        //TODO: replace this with parser lookup, otherwise its impossible to link thumbs without knowing too much about the consumers
                        //We might want to resort to parsing the file name and
                        //then finding it via episodes incase the file names get out of sync
                        var episodeFile = _mediaFileService.FindByPath(possibleMetadataFile, false);

                        if (episodeFile == null) break;

                        metadata.EpisodeFileId = episodeFile.Id;
                    }

                    _metadataFileService.Upsert(metadata);
                }
            }
        }
    }
}
