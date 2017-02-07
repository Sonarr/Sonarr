using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
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
            var seriesRootPaths = _seriesService.GetAllSeries()
                .Where(s => _diskProvider.FolderExists(s.Path))
                .Select(s => _diskProvider.GetPathRoot(s.Path))
                .Distinct();

            return GetDiskSpace(seriesRootPaths);
        }

        private IEnumerable<DiskSpace> GetDroneFactoryFreeSpace()
        {
            if (_configService.DownloadedEpisodesFolder.IsNotNullOrWhiteSpace() && _diskProvider.FolderExists(_configService.DownloadedEpisodesFolder))
            {
                return GetDiskSpace(new[] { _diskProvider.GetPathRoot(_configService.DownloadedEpisodesFolder) });
            }

            return new List<DiskSpace>();
        }

        private IEnumerable<DiskSpace> GetFixedDisksFreeSpace()
        {
            return GetDiskSpace(_diskProvider.GetMounts().Where(d => d.DriveType == DriveType.Fixed).Select(d => d.RootDirectory), true);
        }

        private IEnumerable<DiskSpace> GetDiskSpace(IEnumerable<string> paths, bool suppressWarnings = false)
        {
            foreach (var path in paths)
            {
                DiskSpace diskSpace = null;

                try
                {
                    var freeSpace = _diskProvider.GetAvailableSpace(path);
                    var totalSpace = _diskProvider.GetTotalSize(path);

                    if (!freeSpace.HasValue || !totalSpace.HasValue)
                    {
                        continue;
                    }

                    diskSpace = new DiskSpace
                                {
                                    Path = path,
                                    FreeSpace = freeSpace.Value,
                                    TotalSpace = totalSpace.Value
                                };

                    diskSpace.Label = _diskProvider.GetVolumeLabel(path);
                }
                catch (Exception ex)
                {
                    if (!suppressWarnings)
                    {
                        _logger.Warn(ex, "Unable to get free space for: " + path);
                    }
                }

                if (diskSpace != null)
                {
                    yield return diskSpace;
                }
            }
        }
    }
}
