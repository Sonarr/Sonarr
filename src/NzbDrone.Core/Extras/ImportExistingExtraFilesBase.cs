using System.Collections.Generic;
using System.IO;
using System.Linq;
using NzbDrone.Common;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Extras
{
    public abstract class ImportExistingExtraFilesBase<TExtraFile> : IImportExistingExtraFiles
        where TExtraFile : ExtraFile, new()
    {
        private readonly IExtraFileService<TExtraFile> _extraFileService;

        public ImportExistingExtraFilesBase(IExtraFileService<TExtraFile> extraFileService)
        {
            _extraFileService = extraFileService;
        }

        public abstract int Order { get; }
        public abstract IEnumerable<ExtraFile> ProcessFiles(Series series, List<string> filesOnDisk, List<string> importedFiles);

        public virtual ImportExistingExtraFileFilterResult<TExtraFile> FilterAndClean(Series series, List<string> filesOnDisk, List<string> importedFiles)
        {
            var seriesFiles = _extraFileService.GetFilesBySeries(series.Id);

            Clean(series, filesOnDisk, importedFiles, seriesFiles);

            return Filter(series, filesOnDisk, importedFiles, seriesFiles);
        }

        private ImportExistingExtraFileFilterResult<TExtraFile> Filter(Series series, List<string> filesOnDisk, List<string> importedFiles, List<TExtraFile> seriesFiles)
        {
            var previouslyImported = seriesFiles.IntersectBy(s => Path.Combine(series.Path, s.RelativePath), filesOnDisk, f => f, PathEqualityComparer.Instance).ToList();
            var filteredFiles = filesOnDisk.Except(previouslyImported.Select(f => Path.Combine(series.Path, f.RelativePath)).ToList(), PathEqualityComparer.Instance)
                                           .Except(importedFiles, PathEqualityComparer.Instance)
                                           .ToList();

            // Return files that are already imported so they aren't imported again by other importers.
            // Filter out files that were previously imported and as well as ones imported by other importers.
            return new ImportExistingExtraFileFilterResult<TExtraFile>(previouslyImported, filteredFiles);
        }

        private void Clean(Series series, List<string> filesOnDisk, List<string> importedFiles, List<TExtraFile> seriesFiles)
        {
            var alreadyImportedFileIds = seriesFiles.IntersectBy(f => Path.Combine(series.Path, f.RelativePath), importedFiles, i => i, PathEqualityComparer.Instance)
                .Select(f => f.Id);

            var deletedFiles = seriesFiles.ExceptBy(f => Path.Combine(series.Path, f.RelativePath), filesOnDisk, i => i, PathEqualityComparer.Instance)
                .Select(f => f.Id);

            _extraFileService.DeleteMany(alreadyImportedFileIds);
            _extraFileService.DeleteMany(deletedFiles);
        }
    }
}
