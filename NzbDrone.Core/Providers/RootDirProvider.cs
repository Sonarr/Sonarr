using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers
{
    public class RootDirProvider
    {
        private readonly IRepository _repository;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly DiskProvider _diskProvider;
        private readonly SeriesProvider _seriesProvider;


        public RootDirProvider(IRepository repository, SeriesProvider seriesProvider, DiskProvider diskProvider)
        {
            _repository = repository;
            _diskProvider = diskProvider;
            _seriesProvider = seriesProvider;
        }

        #region IRootDirProvider

        public virtual List<RootDir> GetAll()
        {
            return _repository.All<RootDir>().ToList();
        }

        public virtual int Add(RootDir rootDir)
        {
            ValidatePath(rootDir);

            return (int)_repository.Add(rootDir);
        }

        public virtual void Remove(int rootDirId)
        {
            _repository.Delete<RootDir>(rootDirId);
        }

        public virtual void Update(RootDir rootDir)
        {
            ValidatePath(rootDir);

            _repository.Update(rootDir);
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
            return _repository.Single<RootDir>(rootDirId);
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
                    results.Add(cleanPath);
            }

            Logger.Debug("{0} unmapped folders detected.", results.Count);
            return results;
        }

        #endregion
    }
}