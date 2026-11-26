using System.Linq;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class MountCheck : HealthCheckBase
    {
        private readonly IDiskProvider _diskProvider;
        private readonly ISeriesService _seriesService;

        public MountCheck(IDiskProvider diskProvider, ISeriesService seriesService, ILocalizationService localizationService)
            : base(localizationService)
        {
            _diskProvider = diskProvider;
            _seriesService = seriesService;
        }

        public override HealthCheck Check()
        {
            // Due to possible symlinks and junctions, we get mounts based on series path so internals can handle mount resolution.
            var mounts = _diskProvider.GetMountsForPaths(_seriesService.GetAllSeriesPaths().Values.ToHashSet())
                .Where(m => m.Value is { MountOptions.IsReadOnly: true })
                .DistinctBy(m => m.Value.RootDirectory)
                .ToList();

            if (mounts.Any())
            {
                return new HealthCheck(GetType(),
                    HealthCheckResult.Error,
                    HealthCheckReason.MountSeries,
                    $"{_localizationService.GetLocalizedString("MountSeriesHealthCheckMessage")}{string.Join(", ", mounts.Select(m => $"{m.Value.Name} ({m.Key})"))}",
                    "#series-mount-ro");
            }

            return new HealthCheck(GetType());
        }
    }
}
