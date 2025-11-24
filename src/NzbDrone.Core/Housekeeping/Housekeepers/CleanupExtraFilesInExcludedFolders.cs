using System.IO;
using System.Linq;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Extras.Others;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupExtraFilesInExcludedFolders : IHousekeepingTask
    {
        private readonly IExtraFileRepository<OtherExtraFile> _extraFileRepository;
        private readonly ISeriesService _seriesService;
        private readonly IDiskScanService _diskScanService;

        public CleanupExtraFilesInExcludedFolders(IExtraFileRepository<OtherExtraFile> extraFileRepository, ISeriesService seriesService, IDiskScanService diskScanService)
        {
            _extraFileRepository = extraFileRepository;
            _seriesService = seriesService;
            _diskScanService = diskScanService;
        }

        public void Clean()
        {
            var allSeries = _seriesService.GetAllSeries();

            foreach (var series in allSeries)
            {
                var extraFiles = _extraFileRepository.GetFilesBySeries(series.Id);
                var filteredExtraFiles = _diskScanService.FilterPaths(series.Path, extraFiles.Select(e => Path.Combine(series.Path, e.RelativePath)));

                if (filteredExtraFiles.Count == extraFiles.Count)
                {
                    continue;
                }

                var excludedExtraFiles = extraFiles.Where(e => !filteredExtraFiles.Contains(Path.Combine(e.RelativePath))).ToList();

                if (excludedExtraFiles.Any())
                {
                    _extraFileRepository.DeleteMany(excludedExtraFiles.Select(e => e.Id));
                }
            }
        }
    }
}
