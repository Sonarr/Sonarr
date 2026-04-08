using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Extras.Metadata.Files
{
    public interface ICleanMetadataService
    {
        void Clean(Series series);
    }

    public class CleanExtraFileService : ICleanMetadataService
    {
        private readonly IMetadataFileService _metadataFileService;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public CleanExtraFileService(IMetadataFileService metadataFileService,
                                    IDiskProvider diskProvider,
                                    Logger logger)
        {
            _metadataFileService = metadataFileService;
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public void Clean(Series series)
        {
            _logger.Debug("Cleaning missing metadata files for series: {SeriesTitle}", series.Title);

            var metadataFiles = _metadataFileService.GetFilesBySeries(series.Id);

            foreach (var metadataFile in metadataFiles)
            {
                if (!_diskProvider.FileExists(Path.Combine(series.Path, metadataFile.RelativePath)))
                {
                    _logger.Debug("Deleting metadata file from database: {RelativePath}", metadataFile.RelativePath);
                    _metadataFileService.Delete(metadataFile.Id);
                }
            }
        }
    }
}
