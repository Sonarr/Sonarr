using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Extras
{
    public class ExistingExtraFileService : IHandle<SeriesScannedEvent>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly List<IImportExistingExtraFiles> _existingExtraFileImporters;
        private readonly List<IManageExtraFiles> _extraFileManagers;
        private readonly Logger _logger;

        public ExistingExtraFileService(IDiskProvider diskProvider,
                                        List<IImportExistingExtraFiles> existingExtraFileImporters,
                                        List<IManageExtraFiles> extraFileManagers,
                                        Logger logger)
        {
            _diskProvider = diskProvider;
            _existingExtraFileImporters = existingExtraFileImporters.OrderBy(e => e.Order).ToList();
            _extraFileManagers = extraFileManagers.OrderBy(e => e.Order).ToList();
            _logger = logger;
        }

        public void Handle(SeriesScannedEvent message)
        {
            var series = message.Series;
            var extraFiles = new List<ExtraFile>();

            if (!_diskProvider.FolderExists(series.Path))
            {
                return;
            }

            _logger.Debug("Looking for existing extra files in {0}", series.Path);

            var filesOnDisk = _diskProvider.GetFiles(series.Path, SearchOption.AllDirectories);
            var possibleExtraFiles = filesOnDisk.Where(c => !MediaFileExtensions.Extensions.Contains(Path.GetExtension(c).ToLower()) &&
                                                            !c.StartsWith(Path.Combine(series.Path, "EXTRAS"))).ToList();

            var filteredFiles = possibleExtraFiles;
            var importedFiles = new List<string>();

            foreach (var existingExtraFileImporter in _existingExtraFileImporters)
            {
                var imported = existingExtraFileImporter.ProcessFiles(series, filteredFiles, importedFiles);

                importedFiles.AddRange(imported.Select(f => Path.Combine(series.Path, f.RelativePath)));
            }

            _logger.Info("Found {0} extra files", extraFiles);
        }
    }
}
