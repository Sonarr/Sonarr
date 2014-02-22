using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Metadata.Files;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Metadata
{
    public class ExistingMetadataService : IHandle<SeriesUpdatedEvent>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IMetadataFileService _metadataFileService;
        private readonly IParsingService _parsingService;
        private readonly Logger _logger;
        private readonly List<IMetadata> _consumers;

        public ExistingMetadataService(IDiskProvider diskProvider,
                                       IEnumerable<IMetadata> consumers,
                                       IMetadataFileService metadataFileService,
                                       IParsingService parsingService,
                                       Logger logger)
        {
            _diskProvider = diskProvider;
            _metadataFileService = metadataFileService;
            _parsingService = parsingService;
            _logger = logger;
            _consumers = consumers.ToList();
        }

        public void Handle(SeriesUpdatedEvent message)
        {
            if (!_diskProvider.FolderExists(message.Series.Path)) return;

            _logger.Trace("Looking for existing metadata in {0}", message.Series.Path);

            var filesOnDisk = _diskProvider.GetFiles(message.Series.Path, SearchOption.AllDirectories);
            var possibleMetadataFiles = filesOnDisk.Where(c => !MediaFileExtensions.Extensions.Contains(Path.GetExtension(c).ToLower())).ToList();
            var filteredFiles = _metadataFileService.FilterExistingFiles(possibleMetadataFiles, message.Series);

            var metadataFiles = new List<MetadataFile>();

            foreach (var possibleMetadataFile in filteredFiles)
            {
                foreach (var consumer in _consumers)
                {
                    var metadata = consumer.FindMetadataFile(message.Series, possibleMetadataFile);

                    if (metadata == null) continue;

                    if (metadata.Type == MetadataType.EpisodeImage ||
                        metadata.Type == MetadataType.EpisodeMetadata)
                    {
                        var localEpisode = _parsingService.GetLocalEpisode(possibleMetadataFile, message.Series, false);

                        if (localEpisode == null)
                        {
                            _logger.Trace("Cannot find related episodes for: {0}", possibleMetadataFile);
                            break;
                        }

                        if (localEpisode.Episodes.DistinctBy(e => e.EpisodeFileId).Count() > 1)
                        {
                            _logger.Trace("Metadata file: {0} does not match existing files.", possibleMetadataFile);
                            break;
                        }

                        metadata.EpisodeFileId = localEpisode.Episodes.First().EpisodeFileId;
                    }

                    metadataFiles.Add(metadata);
                }
            }

            _metadataFileService.Upsert(metadataFiles);
        }
    }
}
