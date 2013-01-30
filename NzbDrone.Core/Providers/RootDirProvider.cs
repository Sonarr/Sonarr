using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Repository;
using PetaPoco;

namespace NzbDrone.Core.Providers
{
    public class RootDirProvider
    {
        private readonly IDatabase _database;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly DiskProvider _diskProvider;
        private readonly SeriesProvider _seriesProvider;

        public RootDirProvider(IDatabase database, SeriesProvider seriesProvider, DiskProvider diskProvider)
        {
            _database = database;
            _diskProvider = diskProvider;
            _seriesProvider = seriesProvider;
        }

        public virtual List<RootDir> GetAll()
        {
            return _database.Fetch<RootDir>();
        }

        public virtual RootDir Add(RootDir rootDir)
        {
            if (String.IsNullOrWhiteSpace(rootDir.Path) || !Path.IsPathRooted(rootDir.Path))
                throw new ArgumentException("Invalid path");

            if (!_diskProvider.FolderExists(rootDir.Path))
                throw new DirectoryNotFoundException("Can't add root directory that doesn't exist.");

            if (GetAll().Exists(r => DiskProvider.PathEquals(r.Path, rootDir.Path)))
                throw new InvalidOperationException("Root directory already exist.");

            var id = _database.Insert(rootDir);
            rootDir.Id = Convert.ToInt32(id);
            rootDir.FreeSpace = _diskProvider.FreeDiskSpace(new DirectoryInfo(rootDir.Path));

            return rootDir;
        }

        public virtual void Remove(int rootDirId)
        {
            _database.Delete<RootDir>(rootDirId);
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

        public virtual List<RootDir> AllWithFreeSpace()
        {
            var rootDirs = GetAll();

            foreach (var rootDir in rootDirs)
            {
                rootDir.FreeSpace = _diskProvider.FreeDiskSpace(new DirectoryInfo(rootDir.Path));
                rootDir.UnmappedFolders = GetUnmappedFolders(rootDir.Path);
            }

            return rootDirs;
        }

        public virtual Dictionary<string, ulong> FreeSpaceOnDrives()
        {
            var freeSpace = new Dictionary<string, ulong>();

            var rootDirs = GetAll();

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