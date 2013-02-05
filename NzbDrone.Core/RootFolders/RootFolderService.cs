using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.RootFolders
{
    public interface IRootFolderService
    {
        List<RootFolder> All();
        RootFolder Add(RootFolder rootDir);
        void Remove(int rootDirId);
        List<String> GetUnmappedFolders(string path);
        Dictionary<string, ulong> FreeSpaceOnDrives();
    }

    public class RootFolderService : IRootFolderService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IRootFolderRepository _rootFolderRepository;
        private readonly DiskProvider _diskProvider;
        private readonly SeriesProvider _seriesProvider;

        public RootFolderService(IRootFolderRepository rootFolderRepository, SeriesProvider seriesProvider, DiskProvider diskProvider)
        {
            _rootFolderRepository = rootFolderRepository;
            _diskProvider = diskProvider;
            _seriesProvider = seriesProvider;
        }

        public virtual List<RootFolder> All()
        {
            return _rootFolderRepository.All();
        }

        public virtual RootFolder Add(RootFolder rootDir)
        {
            if (String.IsNullOrWhiteSpace(rootDir.Path) || !Path.IsPathRooted(rootDir.Path))
                throw new ArgumentException("Invalid path");

            if (!_diskProvider.FolderExists(rootDir.Path))
                throw new DirectoryNotFoundException("Can't add root directory that doesn't exist.");

            if (All().Exists(r => DiskProvider.PathEquals(r.Path, rootDir.Path)))
                throw new InvalidOperationException("Root directory already exist.");

            _rootFolderRepository.Add(rootDir);

            rootDir.FreeSpace = _diskProvider.FreeDiskSpace(new DirectoryInfo(rootDir.Path));
            rootDir.UnmappedFolders = GetUnmappedFolders(rootDir.Path);
            return rootDir;
        }

        public virtual void Remove(int rootDirId)
        {
            _rootFolderRepository.Delete(rootDirId);
        }

        public virtual List<String> GetUnmappedFolders(string path)
        {
            Logger.Debug("Generating list of unmapped folders");
            if (String.IsNullOrEmpty(path))
                throw new ArgumentException("Invalid path provided", "path");

            var results = new List<String>();

            if (!_diskProvider.FolderExists(path))
            {
                Logger.Debug("Path supplied does not exist: {0}", path);
                return results;
            }

            foreach (string seriesFolder in _diskProvider.GetDirectories(path))
            {
                if (!_seriesProvider.SeriesPathExists(seriesFolder))
                {
                    results.Add(new DirectoryInfo(seriesFolder.Normalize()).Name);
                }
            }

            Logger.Debug("{0} unmapped folders detected.", results.Count);
            return results;
        }


        public virtual Dictionary<string, ulong> FreeSpaceOnDrives()
        {
            var freeSpace = new Dictionary<string, ulong>();

            var rootDirs = All();

            foreach (var rootDir in rootDirs)
            {
                var pathRoot = _diskProvider.GetPathRoot(rootDir.Path);

                if (!freeSpace.ContainsKey(pathRoot))
                {
                    try
                    {
                        freeSpace.Add(pathRoot, _diskProvider.FreeDiskSpace(new DirectoryInfo(rootDir.Path)));
                    }
                    catch (Exception ex)
                    {
                        Logger.WarnException("Error getting fromm space for: " + pathRoot, ex);
                    }
                }
            }

            return freeSpace;
        }
    }
}