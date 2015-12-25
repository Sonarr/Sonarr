using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Extras.Metadata
{
    public class ExistingMetadataImporter : ImportExistingExtraFilesBase<MetadataFile>
    {
        private readonly IExtraFileService<MetadataFile> _metadataFileService;
        private readonly IParsingService _parsingService;
        private readonly Logger _logger;
        private readonly List<IMetadata> _consumers;

        public ExistingMetadataImporter(IExtraFileService<MetadataFile> metadataFileService,
                                        IEnumerable<IMetadata> consumers,
                                        IParsingService parsingService,
                                        Logger logger)
        : base(metadataFileService)
        {
            _metadataFileService = metadataFileService;
            _parsingService = parsingService;
            _logger = logger;
            _consumers = consumers.ToList();
        }

        public override int Order
        {
            get
            {
                return 0;
            }
        }

        public override IEnumerable<ExtraFile> ProcessFiles(Series series, List<string> filesOnDisk, List<string> importedFiles)
        {
            _logger.Debug("Looking for existing metadata in {0}", series.Path);

            var metadataFiles = new List<MetadataFile>();
            var filteredFiles = FilterAndClean(series, filesOnDisk, importedFiles);

            foreach (var possibleMetadataFile in filteredFiles)
            {
                foreach (var consumer in _consumers)
                {
                    var metadata = consumer.FindMetadataFile(series, possibleMetadataFile);

                    if (metadata == null)
                    {
                        continue;
                    }

                    if (metadata.Type == MetadataType.EpisodeImage ||
                        metadata.Type == MetadataType.EpisodeMetadata)
                    {
                        var localEpisode = _parsingService.GetLocalEpisode(possibleMetadataFile, series);

                        if (localEpisode == null)
                        {
                            _logger.Debug("Unable to parse extra file: {0}", possibleMetadataFile);
                            continue;
                        }

                        if (localEpisode.Episodes.Empty())
                        {
                            _logger.Debug("Cannot find related episodes for: {0}", possibleMetadataFile);
                            continue;
                        }

                        if (localEpisode.Episodes.DistinctBy(e => e.EpisodeFileId).Count() > 1)
                        {
                            _logger.Debug("Extra file: {0} does not match existing files.", possibleMetadataFile);
                            continue;
                        }

                        metadata.SeasonNumber = localEpisode.SeasonNumber;
                        metadata.EpisodeFileId = localEpisode.Episodes.First().EpisodeFileId;
                        metadata.Extension = Path.GetExtension(possibleMetadataFile);
                    }

                    metadataFiles.Add(metadata);
                }
            }

            _logger.Info("Found {0} existing metadata files", metadataFiles.Count);
            _metadataFileService.Upsert(metadataFiles);

            return metadataFiles;
        }
    }
}
