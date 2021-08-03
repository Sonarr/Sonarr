using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download;

namespace NzbDrone.Core.RemotePathMappings
{
    public interface IRemotePathMappingService
    {
        List<RemotePathMapping> All();
        RemotePathMapping Add(RemotePathMapping mapping);
        void Remove(int id);
        RemotePathMapping Get(int id);
        RemotePathMapping Update(RemotePathMapping mapping);

        OsPath RemapRemoteToLocal(string host, OsPath remotePath);
        OsPath RemapLocalToRemote(string host, OsPath localPath);
    }

    public class RemotePathMappingService : IRemotePathMappingService
    {
        private readonly IRemotePathMappingRepository _remotePathMappingRepository;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        private readonly ICached<List<RemotePathMapping>> _cache;

        public RemotePathMappingService(IDownloadClientRepository downloadClientRepository,
                                        IRemotePathMappingRepository remotePathMappingRepository,
                                        IDiskProvider diskProvider,
                                        ICacheManager cacheManager,
                                        Logger logger)
        {
            _remotePathMappingRepository = remotePathMappingRepository;
            _diskProvider = diskProvider;
            _logger = logger;

            _cache = cacheManager.GetCache<List<RemotePathMapping>>(GetType());
        }

        public List<RemotePathMapping> All()
        {
            return _cache.Get("all", () => _remotePathMappingRepository.All().ToList(), TimeSpan.FromSeconds(10));
        }

        public RemotePathMapping Add(RemotePathMapping mapping)
        {
            mapping.LocalPath = new OsPath(mapping.LocalPath).AsDirectory().FullPath;
            mapping.RemotePath = new OsPath(mapping.RemotePath).AsDirectory().FullPath;

            var all = All();

            ValidateMapping(all, mapping);

            var result = _remotePathMappingRepository.Insert(mapping);

            _cache.Clear();

            return result;
        }

        public void Remove(int id)
        {
            _remotePathMappingRepository.Delete(id);

            _cache.Clear();
        }

        public RemotePathMapping Get(int id)
        {
            return _remotePathMappingRepository.Get(id);
        }

        public RemotePathMapping Update(RemotePathMapping mapping)
        {
            var existing = All().Where(v => v.Id != mapping.Id).ToList();

            ValidateMapping(existing, mapping);

            var result = _remotePathMappingRepository.Update(mapping);

            _cache.Clear();

            return result;
        }

        private void ValidateMapping(List<RemotePathMapping> existing, RemotePathMapping mapping)
        {
            if (mapping.Host.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("Invalid Host");
            }

            var remotePath = new OsPath(mapping.RemotePath);
            var localPath = new OsPath(mapping.LocalPath);

            if (remotePath.IsEmpty)
            {
                throw new ArgumentException("Invalid RemotePath");
            }

            if (localPath.IsEmpty || !localPath.IsRooted)
            {
                throw new ArgumentException("Invalid LocalPath");
            }

            if (!_diskProvider.FolderExists(localPath.FullPath))
            {
                throw new DirectoryNotFoundException("Can't add mount point directory that doesn't exist.");
            }

            if (existing.Exists(r => r.Host == mapping.Host && r.RemotePath == mapping.RemotePath))
            {
                throw new InvalidOperationException("RemotePath already mounted.");
            }
        }

        public OsPath RemapRemoteToLocal(string host, OsPath remotePath)
        {
            if (remotePath.IsEmpty)
            {
                return remotePath;
            }

            foreach (var mapping in All())
            {
                if (host.Equals(mapping.Host, StringComparison.InvariantCultureIgnoreCase) && new OsPath(mapping.RemotePath).Contains(remotePath))
                {
                    var localPath = new OsPath(mapping.LocalPath) + (remotePath - new OsPath(mapping.RemotePath));

                    return localPath;
                }
            }

            return remotePath;
        }

        public OsPath RemapLocalToRemote(string host, OsPath localPath)
        {
            if (localPath.IsEmpty)
            {
                return localPath;
            }

            foreach (var mapping in All())
            {
                if (host.Equals(mapping.Host, StringComparison.InvariantCultureIgnoreCase) && new OsPath(mapping.LocalPath).Contains(localPath))
                {
                    var remotePath = new OsPath(mapping.RemotePath) + (localPath - new OsPath(mapping.LocalPath));

                    return remotePath;
                }
            }

            return localPath;
        }
    }
}
