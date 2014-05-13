using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Tv;
using NzbDrone.Common.Cache;
using NzbDrone.Core.Download;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.RemotePathMappings
{
    public interface IRemotePathMappingService
    {
        List<RemotePathMapping> All();
        RemotePathMapping Add(RemotePathMapping mapping);
        void Remove(int id);
        RemotePathMapping Get(int id);
        RemotePathMapping Update(RemotePathMapping mapping);

        String RemapRemoteToLocal(String host, String remotePath);
        String RemapLocalToRemote(String host, String localPath);

        // TODO: Remove around January 2015. Used to migrate legacy Local Category Path settings.
        void MigrateLocalCategoryPath(Int32 downloadClientId, IProviderConfig newSettings, String host, String remotePath, String localPath);
    }

    public class RemotePathMappingService : IRemotePathMappingService
    {
        // TODO: Remove DownloadClientRepository reference around January 2015. Used to migrate legacy Local Category Path settings.
        private readonly IDownloadClientRepository _downloadClientRepository;
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
            _downloadClientRepository = downloadClientRepository;
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
            mapping.LocalPath = CleanPath(mapping.LocalPath);
            mapping.RemotePath = CleanPath(mapping.RemotePath);

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

            if (mapping.RemotePath.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("Invalid RemotePath");
            }

            if (mapping.LocalPath.IsNullOrWhiteSpace() || !Path.IsPathRooted(mapping.LocalPath))
            {
                throw new ArgumentException("Invalid LocalPath");
            }

            if (!_diskProvider.FolderExists(mapping.LocalPath))
            {
                throw new DirectoryNotFoundException("Can't add mount point directory that doesn't exist.");
            }

            if (existing.Exists(r => r.Host == mapping.Host && r.RemotePath == mapping.RemotePath))
            {
                throw new InvalidOperationException("RemotePath already mounted.");
            }
        }

        public String RemapRemoteToLocal(String host, String remotePath)
        {
            if (remotePath.IsNullOrWhiteSpace())
            {
                return remotePath;
            }

            var cleanRemotePath = CleanPath(remotePath);

            foreach (var mapping in All())
            {
                if (host == mapping.Host && cleanRemotePath.StartsWith(mapping.RemotePath))
                {
                    var localPath = mapping.LocalPath + cleanRemotePath.Substring(mapping.RemotePath.Length);

                    localPath = localPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

                    if (!remotePath.EndsWith("/") && !remotePath.EndsWith("\\"))
                    {
                        localPath = localPath.TrimEnd('/', '\\');
                    }

                    return localPath;
                }
            }

            return remotePath;
        }

        public String RemapLocalToRemote(String host, String localPath)
        {
            if (localPath.IsNullOrWhiteSpace())
            {
                return localPath;
            }

            var cleanLocalPath = CleanPath(localPath);

            foreach (var mapping in All())
            {
                if (host != mapping.Host) continue;

                if (cleanLocalPath.StartsWith(mapping.LocalPath))
                {
                    var remotePath = mapping.RemotePath + cleanLocalPath.Substring(mapping.LocalPath.Length);

                    remotePath = remotePath.Replace(Path.DirectorySeparatorChar, mapping.RemotePath.Contains('\\') ? '\\' : '/');

                    if (!localPath.EndsWith("/") && !localPath.EndsWith("\\"))
                    {
                        remotePath = remotePath.TrimEnd('/', '\\');
                    }

                    return remotePath;
                }
            }

            return localPath;
        }

        // TODO: Remove around January 2015. Used to migrate legacy Local Category Path settings.
        public void MigrateLocalCategoryPath(Int32 downloadClientId, IProviderConfig newSettings, String host, String remotePath, String localPath)
        {
            _logger.Debug("Migrating local category path for Host {0}/{1} to {2}", host, remotePath, localPath);

            var existingMappings = All().Where(v => v.Host == host).ToList();

            remotePath = CleanPath(remotePath);
            localPath = CleanPath(localPath);

            if (!existingMappings.Any(v => v.LocalPath == localPath && v.RemotePath == remotePath))
            {
                Add(new RemotePathMapping { Host = host, RemotePath = remotePath, LocalPath = localPath });
            }

            var downloadClient = _downloadClientRepository.Get(downloadClientId);
            downloadClient.Settings = newSettings;
            _downloadClientRepository.Update(downloadClient);
        }

        private static String CleanPath(String path)
        {
            if (path.StartsWith(@"\\") || path.Contains(':'))
            {
                return path.TrimEnd('\\', '/').Replace('/', '\\') + "\\";
            }
            else
            {
                return path.TrimEnd('\\', '/').Replace('\\', '/') + "/";
            }
        }
    }
}