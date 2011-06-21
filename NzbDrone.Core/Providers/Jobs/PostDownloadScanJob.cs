using System;
using System.IO;
using Ninject;
using NLog;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers.Jobs
{
    public class PostDownloadScanJob : IJob
    {
        private readonly ConfigProvider _configProvider;
        private readonly DiskProvider _diskProvider;
        private readonly DiskScanProvider _diskScanProvider;
        private readonly SeriesProvider _seriesProvider;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public PostDownloadScanJob(ConfigProvider configProvider, DiskProvider diskProvider,
                                    DiskScanProvider diskScanProvider, SeriesProvider seriesProvider)
        {
            _configProvider = configProvider;
            _diskProvider = diskProvider;
            _diskScanProvider = diskScanProvider;
            _seriesProvider = seriesProvider;
        }

        public PostDownloadScanJob()
        {
        }

        public string Name
        {
            get { return "Post Download Media File Scan"; }
        }

        public int DefaultInterval
        {
            get { return 1; }
        }

        public virtual void Start(ProgressNotification notification, int targetId)
        {
            Logger.Debug("Starting New Download Scan Job");
            var dropFolder = _configProvider.SabDropDirectory;

            if (String.IsNullOrWhiteSpace(dropFolder))
            {
                Logger.Debug("Skipping drop folder scan. No drop folder is defined.");
                return;
            }

            if (!_diskProvider.FolderExists(dropFolder))
            {
                Logger.Warn("Unable to Scan for New Downloads - folder Doesn't exist: {0}", dropFolder);
                return;
            }

            foreach (var subfolder in _diskProvider.GetDirectories(dropFolder))
            {
                var subfolderInfo = new DirectoryInfo(subfolder);

                if (subfolderInfo.Name.StartsWith("_UNPACK_", StringComparison.CurrentCultureIgnoreCase))
                {
                    Logger.Info("Folder [{0}] is still being unpacked. skipping.", subfolder);
                    continue;
                }

                if (subfolderInfo.Name.StartsWith("_FAILED_", StringComparison.CurrentCultureIgnoreCase))
                {
                    Logger.Info("Folder [{0}] is marked as failed. skipping.", subfolder);
                    continue;
                }

                //Parse the Folder name
                var seriesName = Parser.ParseSeriesName(subfolderInfo.Name);
                var series = _seriesProvider.FindSeries(seriesName);

                if (series == null)
                {
                    Logger.Warn("Unable to Import new download, series is not being watched");
                    return;
                }

                var importedFiles = _diskScanProvider.Scan(series, subfolder);
                importedFiles.ForEach(file => _diskScanProvider.RenameEpisodeFile(file));
            }

            Logger.Debug("New Download Scan Job completed successfully");
        }
    }
}
