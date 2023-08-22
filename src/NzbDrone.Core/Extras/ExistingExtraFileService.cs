using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Extras
{
    public interface IExistingExtraFiles : IHandle<SeriesScannedEvent>
    {
        List<string> ImportFileList(Series series, List<string> possibleExtraFiles);
    }

    public class ExistingExtraFileService : IExistingExtraFiles
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IDiskScanService _diskScanService;
        private readonly List<IImportExistingExtraFiles> _existingExtraFileImporters;
        private readonly Logger _logger;

        public ExistingExtraFileService(IDiskProvider diskProvider,
                                        IDiskScanService diskScanService,
                                        IEnumerable<IImportExistingExtraFiles> existingExtraFileImporters,
                                        Logger logger)
        {
            _diskProvider = diskProvider;
            _diskScanService = diskScanService;
            _existingExtraFileImporters = existingExtraFileImporters.OrderBy(e => e.Order).ToList();
            _logger = logger;
        }

        public List<string> ImportFileList(Series series, List<string> possibleExtraFiles)
        {
            _logger.Debug("Looking for existing extra files in {0}", series.Path);

            var importedFiles = new List<string>();

            foreach (var existingExtraFileImporter in _existingExtraFileImporters)
            {
                var imported = existingExtraFileImporter.ProcessFiles(series, possibleExtraFiles, importedFiles);

                importedFiles.AddRange(imported.Select(f => Path.Combine(series.Path, f.RelativePath)));
            }

            return importedFiles;
        }

        public void Handle(SeriesScannedEvent message)
        {
            var series = message.Series;

            if (!_diskProvider.FolderExists(series.Path))
            {
                return;
            }

            var filesOnDisk = _diskScanService.GetNonVideoFiles(series.Path);
            var possibleExtraFiles = _diskScanService.FilterPaths(series.Path, filesOnDisk);

            var importedFiles = ImportFileList(series, possibleExtraFiles);

            _logger.Info("Found {0} possible extra files, imported {1} files.", possibleExtraFiles.Count, importedFiles.Count);
        }
    }
}
