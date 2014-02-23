using System.Collections.Generic;
using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Metadata.Files
{
    public interface ICleanMetadataService
    {
        void Clean(Series series);
    }

    public class CleanMetadataService : ICleanMetadataService
    {
        private readonly IMetadataFileService _metadataFileService;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public CleanMetadataService(IMetadataFileService metadataFileService,
                                      IDiskProvider diskProvider,
                                      Logger logger)
        {
            _metadataFileService = metadataFileService;
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public void Clean(Series series)
        {
            _logger.Trace("Cleaning missing metadata files for series: {0}", series.Title);

            var metadataFiles = _metadataFileService.GetFilesBySeries(series.Id);

            foreach (var metadataFile in metadataFiles)
            {
                if (!_diskProvider.FileExists(Path.Combine(series.Path, metadataFile.RelativePath)))
                {
                    _logger.Trace("Deleting metadata file from database: {0}", metadataFile.RelativePath);
                    _metadataFileService.Delete(metadataFile.Id);
                }
            }
        }
    }
}
