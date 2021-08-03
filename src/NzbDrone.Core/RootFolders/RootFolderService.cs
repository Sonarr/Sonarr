using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.RootFolders
{
    public interface IRootFolderService
    {
        List<RootFolder> All();
        List<RootFolder> AllWithUnmappedFolders();
        RootFolder Add(RootFolder rootDir);
        void Remove(int id);
        RootFolder Get(int id, bool timeout);
        string GetBestRootFolderPath(string path);
    }

    public class RootFolderService : IRootFolderService
    {
        private readonly IRootFolderRepository _rootFolderRepository;
        private readonly IDiskProvider _diskProvider;
        private readonly ISeriesRepository _seriesRepository;
        private readonly Logger _logger;

        private static readonly HashSet<string> SpecialFolders = new HashSet<string>
                                                                 {
                                                                     "$recycle.bin",
                                                                     "system volume information",
                                                                     "recycler",
                                                                     "lost+found",
                                                                     ".appledb",
                                                                     ".appledesktop",
                                                                     ".appledouble",
                                                                     "@eadir",
                                                                     ".grab"
                                                                 };

        public RootFolderService(IRootFolderRepository rootFolderRepository,
                                 IDiskProvider diskProvider,
                                 ISeriesRepository seriesRepository,
                                 Logger logger)
        {
            _rootFolderRepository = rootFolderRepository;
            _diskProvider = diskProvider;
            _seriesRepository = seriesRepository;
            _logger = logger;
        }

        public List<RootFolder> All()
        {
            var rootFolders = _rootFolderRepository.All().ToList();

            return rootFolders;
        }

        public List<RootFolder> AllWithUnmappedFolders()
        {
            var rootFolders = _rootFolderRepository.All().ToList();
            var seriesPaths = _seriesRepository.AllSeriesPaths();

            rootFolders.ForEach(folder =>
            {
                try
                {
                    if (folder.Path.IsPathValid())
                    {
                        GetDetails(folder, seriesPaths, true);
                    }
                }

                //We don't want an exception to prevent the root folders from loading in the UI, so they can still be deleted
                catch (Exception ex)
                {
                    _logger.Error(ex, "Unable to get free space and unmapped folders for root folder {0}", folder.Path);
                    folder.UnmappedFolders = new List<UnmappedFolder>();
                }
            });

            return rootFolders;
        }

        public RootFolder Add(RootFolder rootFolder)
        {
            var all = All();

            if (string.IsNullOrWhiteSpace(rootFolder.Path) || !Path.IsPathRooted(rootFolder.Path))
            {
                throw new ArgumentException("Invalid path");
            }

            if (!_diskProvider.FolderExists(rootFolder.Path))
            {
                throw new DirectoryNotFoundException("Can't add root directory that doesn't exist.");
            }

            if (all.Exists(r => r.Path.PathEquals(rootFolder.Path)))
            {
                throw new InvalidOperationException("Recent directory already exists.");
            }

            if (!_diskProvider.FolderWritable(rootFolder.Path))
            {
                throw new UnauthorizedAccessException(string.Format("Root folder path '{0}' is not writable by user '{1}'", rootFolder.Path, Environment.UserName));
            }

            _rootFolderRepository.Insert(rootFolder);
            var seriesPaths = _seriesRepository.AllSeriesPaths();

            GetDetails(rootFolder, seriesPaths, true);

            return rootFolder;
        }

        public void Remove(int id)
        {
            _rootFolderRepository.Delete(id);
        }

        private List<UnmappedFolder> GetUnmappedFolders(string path, List<string> seriesPaths)
        {
            _logger.Debug("Generating list of unmapped folders");

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Invalid path provided", nameof(path));
            }

            var results = new List<UnmappedFolder>();

            if (!_diskProvider.FolderExists(path))
            {
                _logger.Debug("Path supplied does not exist: {0}", path);
                return results;
            }

            var possibleSeriesFolders = _diskProvider.GetDirectories(path).ToList();
            var unmappedFolders = possibleSeriesFolders.Except(seriesPaths, PathEqualityComparer.Instance).ToList();

            foreach (string unmappedFolder in unmappedFolders)
            {
                var di = new DirectoryInfo(unmappedFolder.Normalize());
                results.Add(new UnmappedFolder { Name = di.Name, Path = di.FullName });
            }

            var setToRemove = SpecialFolders;
            results.RemoveAll(x => setToRemove.Contains(new DirectoryInfo(x.Path.ToLowerInvariant()).Name));

            _logger.Debug("{0} unmapped folders detected.", results.Count);
            return results.OrderBy(u => u.Name, StringComparer.InvariantCultureIgnoreCase).ToList();
        }

        public RootFolder Get(int id, bool timeout)
        {
            var rootFolder = _rootFolderRepository.Get(id);
            var seriesPaths = _seriesRepository.AllSeriesPaths();

            GetDetails(rootFolder, seriesPaths, timeout);

            return rootFolder;
        }

        public string GetBestRootFolderPath(string path)
        {
            var possibleRootFolder = All().Where(r => r.Path.IsParentPath(path))
                                          .OrderByDescending(r => r.Path.Length)
                                          .FirstOrDefault();

            if (possibleRootFolder == null)
            {
                return _diskProvider.GetParentFolder(path);
            }

            return possibleRootFolder.Path;
        }

        private void GetDetails(RootFolder rootFolder, List<string> seriesPaths, bool timeout)
        {
            Task.Run(() =>
            {
                if (_diskProvider.FolderExists(rootFolder.Path))
                {
                    rootFolder.Accessible = true;
                    rootFolder.FreeSpace = _diskProvider.GetAvailableSpace(rootFolder.Path);
                    rootFolder.TotalSpace = _diskProvider.GetTotalSize(rootFolder.Path);
                    rootFolder.UnmappedFolders = GetUnmappedFolders(rootFolder.Path, seriesPaths);
                }
            }).Wait(timeout ? 5000 : -1);
        }
    }
}
