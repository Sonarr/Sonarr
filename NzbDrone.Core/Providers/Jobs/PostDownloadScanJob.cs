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
        private readonly MediaFileProvider _mediaFileProvider;
        private readonly SeriesProvider _seriesProvider;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public PostDownloadScanJob(ConfigProvider configProvider, DiskProvider diskProvider,
                                    MediaFileProvider mediaFileProvider, SeriesProvider seriesProvider)
        {
            _configProvider = configProvider;
            _diskProvider = diskProvider;
            _mediaFileProvider = mediaFileProvider;
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

            if (String.IsNullOrEmpty(dropFolder))
            {
                Logger.Warn("Unable to Scan for New Downloads - Folder Name is Empty");
                return;
            }

            if (!_diskProvider.FolderExists(dropFolder))
            {
                Logger.Warn("Unable to Scan for New Downloads - folder Doesn't exist: {0}", dropFolder);
                return;
            }

            var subfolders = _diskProvider.GetDirectories(dropFolder);

            foreach (var subfolder in subfolders)
            {
                var di = new DirectoryInfo(subfolder);

                if (di.Name.StartsWith("_UNPACK_"))
                {
                    Logger.Info("Folder [{0}] is still being unpacked", subfolder);
                    continue;
                }

                if (di.Name.StartsWith("_FAILED_"))
                {
                    Logger.Info("Folder [{0}] is marked as failed", subfolder);
                    continue;
                }

                //Parse the Folder name
                var seriesName = Parser.ParseSeriesName(di.Name);
                var series = _seriesProvider.FindSeries(seriesName);

                if (series == null)
                {
                    Logger.Warn("Unable to Import new download, series is not being watched");
                    return;
                }

                _mediaFileProvider.ImportNewFiles(subfolder, series);
            }
            Logger.Debug("New Download Scan Job completed successfully");
        }
    }
}
