using System;
using System.IO;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Core.Tv
{
    public interface IBuildSeriesPaths
    {
        string BuildPath(Series series, bool useExistingRelativeFolder);
    }

    public class SeriesPathBuilder : IBuildSeriesPaths
    {
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly IRootFolderService _rootFolderService;
        private readonly Logger _logger;

        public SeriesPathBuilder(IBuildFileNames fileNameBuilder, IRootFolderService rootFolderService, Logger logger)
        {
            _fileNameBuilder = fileNameBuilder;
            _rootFolderService = rootFolderService;
            _logger = logger;
        }

        public string BuildPath(Series series, bool useExistingRelativeFolder)
        {
            if (series.RootFolderPath.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("Root folder was not provided", nameof(series));
            }

            if (useExistingRelativeFolder && series.Path.IsNotNullOrWhiteSpace())
            {
                var relativePath = GetExistingRelativePath(series);
                return Path.Combine(series.RootFolderPath, relativePath);
            }

            return Path.Combine(series.RootFolderPath, _fileNameBuilder.GetSeriesFolder(series));
        }

        private string GetExistingRelativePath(Series series)
        {
            var rootFolderPath = _rootFolderService.GetBestRootFolderPath(series.Path);

            if (rootFolderPath.IsParentPath(series.Path))
            {
                return rootFolderPath.GetRelativePath(series.Path);
            }

            var directoryName = series.Path.GetDirectoryName();

            _logger.Warn("Unable to get relative path for series path {0}, using series folder name {1}", series.Path, directoryName);

            return directoryName;
        }
    }
}
