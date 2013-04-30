using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.RootFolders
{
    public interface IRootFolderService
    {
        List<RootFolder> All();
        RootFolder Add(RootFolder rootDir);
        void Remove(int id);
        List<UnmappedFolder> GetUnmappedFolders(string path);
        Dictionary<string, long> FreeSpaceOnDrives();
        RootFolder Get(int id);
    }

    public class RootFolderService : IRootFolderService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IBasicRepository<RootFolder> _rootFolderRepository;
        private readonly DiskProvider _diskProvider;
        private readonly ISeriesRepository _seriesRepository;

        public RootFolderService(IBasicRepository<RootFolder> rootFolderRepository, DiskProvider diskProvider,ISeriesRepository seriesRepository)
        {
            _rootFolderRepository = rootFolderRepository;
            _diskProvider = diskProvider;
            _seriesRepository = seriesRepository;
        }

        public virtual List<RootFolder> All()
        {
            var rootFolders = _rootFolderRepository.All().ToList();

            rootFolders.ForEach(folder =>
                {
                    if (_diskProvider.FolderExists(folder.Path))
                    {
                        folder.FreeSpace = _diskProvider.GetAvilableSpace(folder.Path);
                        folder.UnmappedFolders = GetUnmappedFolders(folder.Path);
                    }
                });

            return rootFolders;
        }

        public virtual RootFolder Add(RootFolder rootFolder)
        {
            if (String.IsNullOrWhiteSpace(rootFolder.Path) || !Path.IsPathRooted(rootFolder.Path))
                throw new ArgumentException("Invalid path");

            if (!_diskProvider.FolderExists(rootFolder.Path))
                throw new DirectoryNotFoundException("Can't add root directory that doesn't exist.");

            if (All().Exists(r => DiskProvider.PathEquals(r.Path, rootFolder.Path)))
                throw new InvalidOperationException("Root directory already exist.");

            _rootFolderRepository.Insert(rootFolder);

            rootFolder.FreeSpace = _diskProvider.GetAvilableSpace(rootFolder.Path);
            rootFolder.UnmappedFolders = GetUnmappedFolders(rootFolder.Path);
            return rootFolder;
        }

        public virtual void Remove(int id)
        {
            _rootFolderRepository.Delete(id);
        }

        public virtual List<UnmappedFolder> GetUnmappedFolders(string path)
        {
            Logger.Debug("Generating list of unmapped folders");
            if (String.IsNullOrEmpty(path))
                throw new ArgumentException("Invalid path provided", "path");

            var results = new List<UnmappedFolder>();

            if (!_diskProvider.FolderExists(path))
            {
                Logger.Debug("Path supplied does not exist: {0}", path);
                return results;
            }

            foreach (string seriesFolder in _diskProvider.GetDirectories(path))
            {
                if (!_seriesRepository.SeriesPathExists(seriesFolder))
                {
                    var di = new DirectoryInfo(seriesFolder.Normalize());
                    results.Add(new UnmappedFolder{ Name = di.Name, Path = di.FullName });
                }
            }

            Logger.Debug("{0} unmapped folders detected.", results.Count);
            return results;
        }

        public virtual Dictionary<string, long> FreeSpaceOnDrives()
        {
            var freeSpace = new Dictionary<string, long>();

            var rootDirs = All();

            foreach (var rootDir in rootDirs)
            {
                var pathRoot = _diskProvider.GetPathRoot(rootDir.Path);

                if (!freeSpace.ContainsKey(pathRoot))
                {
                    try
                    {
                        freeSpace.Add(pathRoot, _diskProvider.GetAvilableSpace(rootDir.Path));
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
            return _rootFolderRepository.Get(id);
        }
    }
}