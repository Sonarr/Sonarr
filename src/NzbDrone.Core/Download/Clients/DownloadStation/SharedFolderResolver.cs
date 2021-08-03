using System;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Download.Clients.DownloadStation.Proxies;

namespace NzbDrone.Core.Download.Clients.DownloadStation
{
    public interface ISharedFolderResolver
    {
        OsPath RemapToFullPath(OsPath sharedFolderPath, DownloadStationSettings settings, string serialNumber);
    }

    public class SharedFolderResolver : ISharedFolderResolver
    {
        private readonly IFileStationProxy _proxy;
        private readonly ILogger _logger;

        private ICached<SharedFolderMapping> _cache;

        public SharedFolderResolver(ICacheManager cacheManager,
                                    IFileStationProxy proxy,
                                    Logger logger)
        {
            _proxy = proxy;
            _cache = cacheManager.GetCache<SharedFolderMapping>(GetType());
            _logger = logger;
        }

        private SharedFolderMapping GetPhysicalPath(OsPath sharedFolder, DownloadStationSettings settings)
        {
            try
            {
                return _proxy.GetSharedFolderMapping(sharedFolder.FullPath, settings);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to get shared folder {0} from Disk Station {1}:{2}", sharedFolder, settings.Host, settings.Port);

                throw;
            }
        }

        public OsPath RemapToFullPath(OsPath sharedFolderPath, DownloadStationSettings settings, string serialNumber)
        {
            var index = sharedFolderPath.FullPath.IndexOf('/', 1);
            var sharedFolder = index == -1 ? sharedFolderPath : new OsPath(sharedFolderPath.FullPath.Substring(0, index));

            var mapping = _cache.Get($"{serialNumber}:{sharedFolder}", () => GetPhysicalPath(sharedFolder, settings), TimeSpan.FromHours(1));

            var fullPath = mapping.PhysicalPath + (sharedFolderPath - mapping.SharedFolder);

            return fullPath;
        }
    }
}
