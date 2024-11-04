using System;
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
            // Not best for optimization but due to possible symlinks and junctions, we get mounts based on series path so internals can handle mount resolution.
            var mounts = _seriesService.GetAllSeriesPaths()
                .Select(p => new Tuple<IMount, string>(_diskProvider.GetMount(p.Value), p.Value))
                .Where(m => m.Item1 is { MountOptions.IsReadOnly: true })
                .DistinctBy(m => m.Item1.RootDirectory)
                .ToList();

            if (mounts.Any())
            {
                return new HealthCheck(GetType(),
                    HealthCheckResult.Error,
                    $"{_localizationService.GetLocalizedString("MountSeriesHealthCheckMessage")}{string.Join(", ", mounts.Select(m => $"{m.Item1.Name} ({m.Item2})"))}",
                    "#series-mount-ro");
            }

            return new HealthCheck(GetType());
        }
    }
}
