using System.Linq;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class MountCheck : HealthCheckBase
    {
        private readonly IDiskProvider _diskProvider;
        private readonly ISeriesService _seriesService;

        public MountCheck(IDiskProvider diskProvider, ISeriesService seriesService)
        {
            _diskProvider = diskProvider;
            _seriesService = seriesService;
        }

        public override HealthCheck Check()
        {
            // Not best for optimization but due to possible symlinks and junctions, we get mounts based on series path so internals can handle mount resolution.
            var mounts = _seriesService.GetAllSeries()
                                       .Select(series => _diskProvider.GetMount(series.Path))
                                       .DistinctBy(m => m.RootDirectory)
                                       .Where(m => m.MountOptions != null && m.MountOptions.IsReadOnly)
                                       .ToList();

            if (mounts.Any())
            {
                return new HealthCheck(GetType(), HealthCheckResult.Error, "Mount containing a series path is mounted read-only: " + string.Join(",", mounts.Select(m => m.Name)), "#series-mount-ro");
            }

            return new HealthCheck(GetType());
        }
    }
}
