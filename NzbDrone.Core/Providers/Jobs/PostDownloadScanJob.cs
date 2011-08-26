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
            get { return "Drop folder monitor"; }
        }

        public int DefaultInterval
        {
            get { return 1; }
        }

        public virtual void Start(ProgressNotification notification, int targetId, int secondaryTargetId)
        {
            var dropFolder = _configProvider.SabDropDirectory;

            if (String.IsNullOrWhiteSpace(dropFolder))
            {
                Logger.Debug("No drop folder is defined. Skipping.");
                return;
            }

            if (!_diskProvider.FolderExists(dropFolder))
            {
                Logger.Warn("Unable to Scan for New Downloads - folder Doesn't exist: [{0}]", dropFolder);
                return;
            }

            foreach (var subfolder in _diskProvider.GetDirectories(dropFolder))
            {
                try
                {
                    var subfolderInfo = new DirectoryInfo(subfolder);

                    if (subfolderInfo.Name.StartsWith("_UNPACK_", StringComparison.CurrentCultureIgnoreCase))
                    {
                        Logger.Debug("Folder [{0}] is still being unpacked. skipping.", subfolder);
                        continue;
                    }

                    if (subfolderInfo.Name.StartsWith("_FAILED_", StringComparison.CurrentCultureIgnoreCase))
                    {
                        Logger.Debug("Folder [{0}] is marked as failed. skipping.", subfolder);
                        continue;
                    }

                    if (subfolderInfo.Name.StartsWith("_NzbDrone_", StringComparison.CurrentCultureIgnoreCase))
                    {
                        Logger.Debug("Folder [{0}] is marked as already processedby NzbDrone. skipping.", subfolder);
                        continue;
                    }

                    //Parse the Folder name
                    var seriesName = Parser.ParseSeriesName(subfolderInfo.Name);
                    var series = _seriesProvider.FindSeries(seriesName);

                    if (series == null)
                    {
                        Logger.Warn("Unable to Import new download [{0}], series doesn't exist in database.", subfolder);
                        continue;
                    }

                    var importedFiles = _diskScanProvider.Scan(series, subfolder);
                    importedFiles.ForEach(file => _diskScanProvider.MoveEpisodeFile(file, true));

                    //Delete the folder only if folder is small enough
                    if (_diskProvider.GetDirectorySize(subfolder) < 10.Megabytes())
                        _diskProvider.DeleteFolder(subfolder, true);

                    //Otherwise rename the folder to say it was already processed once by NzbDrone so it will not be continually processed
                    else
                        _diskProvider.MoveDirectory(subfolderInfo.FullName, Path.Combine(subfolderInfo.Parent.FullName, "_NzbDrone_" + subfolderInfo.Name));
                }

                catch (Exception e)
                {
                    Logger.ErrorException("An error has occurred while importing " + subfolder, e);
                }
            }
        }
    }
}