using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.RootFolders
{
    public interface IRootFolderService
    {
        List<RootFolder> All();
        List<RootFolder> AllWithUnmappedFolders();
        RootFolder Add(RootFolder rootDir);
        void Remove(int id);
        List<UnmappedFolder> GetUnmappedFolders(string path);
        Dictionary<string, long?> FreeSpaceOnDrives();
        RootFolder Get(int id);
    }

    public class RootFolderService : IRootFolderService
    {
        private static readonly Logger Logger =  NzbDroneLogger.GetLogger();
        private readonly IBasicRepository<RootFolder> _rootFolderRepository;
        private readonly IDiskProvider _diskProvider;
        private readonly ISeriesRepository _seriesRepository;
        private readonly IConfigService _configService;

        public RootFolderService(IBasicRepository<RootFolder> rootFolderRepository,
                                 IDiskProvider diskProvider,
                                 ISeriesRepository seriesRepository,
                                 IConfigService configService)
        {
            _rootFolderRepository = rootFolderRepository;
            _diskProvider = diskProvider;
            _seriesRepository = seriesRepository;
            _configService = configService;
        }

        public List<RootFolder> All()
        {
            var rootFolders = _rootFolderRepository.All().ToList();

            return rootFolders;
        }

        public List<RootFolder> AllWithUnmappedFolders()
        {
            var rootFolders = _rootFolderRepository.All().ToList();

            rootFolders.ForEach(folder =>
            {
                if (_diskProvider.FolderExists(folder.Path))
                {
                    folder.FreeSpace = _diskProvider.GetAvailableSpace(folder.Path);
                    folder.UnmappedFolders = GetUnmappedFolders(folder.Path);
                }
            });

            return rootFolders;
        }

        public RootFolder Add(RootFolder rootFolder)
        {
            var all = All();

            if (String.IsNullOrWhiteSpace(rootFolder.Path) || !Path.IsPathRooted(rootFolder.Path))
                throw new ArgumentException("Invalid path");

            if (!_diskProvider.FolderExists(rootFolder.Path))
                throw new DirectoryNotFoundException("Can't add root directory that doesn't exist.");

            if (all.Exists(r => r.Path.PathEquals(rootFolder.Path)))
                throw new InvalidOperationException("Recent directory already exists.");

            if (!String.IsNullOrWhiteSpace(_configService.DownloadedEpisodesFolder) &&
                _configService.DownloadedEpisodesFolder.PathEquals(rootFolder.Path))
                throw new InvalidOperationException("Drone Factory folder cannot be used.");

            _rootFolderRepository.Insert(rootFolder);

            rootFolder.FreeSpace = _diskProvider.GetAvailableSpace(rootFolder.Path);
            rootFolder.UnmappedFolders = GetUnmappedFolders(rootFolder.Path);
            return rootFolder;
        }

        public void Remove(int id)
        {
            _rootFolderRepository.Delete(id);
        }

        public List<UnmappedFolder> GetUnmappedFolders(string path)
        {
            Logger.Debug("Generating list of unmapped folders");
            if (String.IsNullOrEmpty(path))
                throw new ArgumentException("Invalid path provided", "path");

            var results = new List<UnmappedFolder>();
            var series = _seriesRepository.All().ToList();

            if (!_diskProvider.FolderExists(path))
            {
                Logger.Debug("Path supplied does not exist: {0}", path);
                return results;
            }

            var seriesFolders = _diskProvider.GetDirectories(path).ToList();
            var unmappedFolders = seriesFolders.Except(series.Select(s => s.Path), new PathEqualityComparer()).ToList();

            foreach (string unmappedFolder in unmappedFolders)
            {
                var di = new DirectoryInfo(unmappedFolder.Normalize());
                results.Add(new UnmappedFolder { Name = di.Name, Path = di.FullName });
            }

            if (Path.GetPathRoot(path).Equals(path, StringComparison.InvariantCultureIgnoreCase))
            {
                var setToRemove = _diskProvider.SpecialFolders;
                results.RemoveAll(x => setToRemove.Contains(new DirectoryInfo(x.Path.ToLowerInvariant()).Name));
            }

            Logger.Debug("{0} unmapped folders detected.", results.Count);
            return results;
        }

        public Dictionary<string, long?> FreeSpaceOnDrives()
        {
            var freeSpace = new Dictionary<string, long?>();

            var rootDirs = All();

            foreach (var rootDir in rootDirs)
            {
                var pathRoot = _diskProvider.GetPathRoot(rootDir.Path);

                if (!freeSpace.ContainsKey(pathRoot))
                {
                    try
                    {
                        freeSpace.Add(pathRoot, _diskProvider.GetAvailableSpace(rootDir.Path));
                    }
                    catch (Exception ex)
                    {
                        Logger.WarnException("Error getting disk space for: " + pathRoot, ex);
                    }
                }
            }

            return freeSpace;
        }

        public RootFolder Get(int id)
        {
            var rootFolder = _rootFolderRepository.Get(id);
            rootFolder.FreeSpace = _diskProvider.GetAvailableSpace(rootFolder.Path);
            rootFolder.UnmappedFolders = GetUnmappedFolders(rootFolder.Path);
            return rootFolder;
        }
    }
}