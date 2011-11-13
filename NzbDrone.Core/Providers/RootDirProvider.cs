using System;
using System.Collections.Generic;
using System.IO;
using Ninject;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Providers.Core;
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

        [Inject]
        public RootDirProvider(IDatabase database, SeriesProvider seriesProvider, DiskProvider diskProvider)
        {
            _database = database;
            _diskProvider = diskProvider;
            _seriesProvider = seriesProvider;
        }

        #region IRootDirProvider

        public virtual List<RootDir> GetAll()
        {
            return _database.Fetch<RootDir>();
        }

        public virtual int Add(RootDir rootDir)
        {
            ValidatePath(rootDir);
            return Convert.ToInt32(_database.Insert(rootDir));
        }

        public virtual void Remove(int rootDirId)
        {
            _database.Delete<RootDir>(rootDirId);
        }

        private static void ValidatePath(RootDir rootDir)
        {
            if (String.IsNullOrWhiteSpace(rootDir.Path) || !Path.IsPathRooted(rootDir.Path))
            {
                throw new ArgumentException("Invalid path");
            }
        }

        public virtual RootDir GetRootDir(int rootDirId)
        {
            return _database.SingleOrDefault<RootDir>(rootDirId);
        }

        public List<String> GetUnmappedFolders(string path)
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
                var cleanPath = Parser.NormalizePath(new DirectoryInfo(seriesFolder).FullName);

                if (!_seriesProvider.SeriesPathExists(cleanPath))
                {
                    results.Add(cleanPath);
                }
            }

            Logger.Debug("{0} unmapped folders detected.", results.Count);
            return results;
        }

        public virtual string GetMostFreeRootDir()
        {
            ulong maxSize = 0;
            var maxPath = String.Empty;

            var rootDirs = GetAll();

            foreach (var rootDir in rootDirs)
            {
                rootDir.FreeSpace = _diskProvider.FreeDiskSpace(new DirectoryInfo(rootDir.Path));
                if (rootDir.FreeSpace > maxSize)
                {
                    maxPath = rootDir.Path;
                    maxSize = rootDir.FreeSpace;
                }
            }

            return maxPath;
        }

        #endregion
    }
}