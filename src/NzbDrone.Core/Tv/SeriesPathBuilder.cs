using System;
using System.IO;
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

        public SeriesPathBuilder(IBuildFileNames fileNameBuilder, IRootFolderService rootFolderService)
        {
            _fileNameBuilder = fileNameBuilder;
            _rootFolderService = rootFolderService;
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

            return rootFolderPath.GetRelativePath(series.Path);
        }
    }
}
