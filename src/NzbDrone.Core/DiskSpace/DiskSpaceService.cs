using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.DiskSpace
{
    public interface IDiskSpaceService
    {
        List<DiskSpace> GetFreeSpace();
    }

    public class DiskSpaceService : IDiskSpaceService
    {
        private readonly ISeriesService _seriesService;
        private readonly IConfigService _configService;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public DiskSpaceService(ISeriesService seriesService, IConfigService configService, IDiskProvider diskProvider, Logger logger)
        {
            _seriesService = seriesService;
            _configService = configService;
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public List<DiskSpace> GetFreeSpace()
        {
            var diskSpace = new List<DiskSpace>();
            diskSpace.AddRange(GetSeriesFreeSpace());
            diskSpace.AddRange(GetDroneFactoryFreeSpace());
            diskSpace.AddRange(GetFixedDisksFreeSpace());

            return diskSpace.DistinctBy(d => d.Path).ToList();
        }

        private IEnumerable<DiskSpace> GetSeriesFreeSpace()
        {
            var seriesRootPaths = _seriesService.GetAllSeries().Select(s => _diskProvider.GetPathRoot(s.Path)).Distinct();

            return GetDiskSpace(seriesRootPaths);
        }

        private IEnumerable<DiskSpace> GetDroneFactoryFreeSpace()
        {
            if (!String.IsNullOrWhiteSpace(_configService.DownloadedEpisodesFolder))
            {
                return GetDiskSpace(new[] { _configService.DownloadedEpisodesFolder });
            }

            return new List<DiskSpace>();
        }

        private IEnumerable<DiskSpace> GetFixedDisksFreeSpace()
        {
            return GetDiskSpace(_diskProvider.GetFixedDrives());
        }

        private IEnumerable<DiskSpace> GetDiskSpace(IEnumerable<String> paths)
        {
            foreach (var path in paths)
            {
                DiskSpace diskSpace = null;

                try
                {
                    var freeSpace = _diskProvider.GetAvailableSpace(path).Value;
                    var totalSpace = _diskProvider.GetTotalSize(path).Value;

                    diskSpace = new DiskSpace
                                {
                                    Path = path,
                                    FreeSpace = freeSpace,
                                    TotalSpace = totalSpace
                                };

                    diskSpace.Label = _diskProvider.GetVolumeLabel(path);
                }
                catch (Exception ex)
                {
                    _logger.WarnException("Unable to get free space for: " + path, ex);
                }

                if (diskSpace != null)
                {
                    yield return diskSpace;
                }
            }
        }
    }
}
