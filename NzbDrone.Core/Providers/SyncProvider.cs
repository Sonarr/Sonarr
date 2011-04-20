using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NLog;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers
{
    public class SyncProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly DiskProvider _diskProvider;
        private readonly SeriesProvider _seriesProvider;

        public SyncProvider(SeriesProvider seriesProvider, DiskProvider diskProvider)
        {
            _seriesProvider = seriesProvider;
            _diskProvider = diskProvider;
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
    }
}