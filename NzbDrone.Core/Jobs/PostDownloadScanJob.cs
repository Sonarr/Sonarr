using System.Linq;
using System;
using NLog;
using Ninject;
using NzbDrone.Common;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Jobs
{
    public class PostDownloadScanJob : IJob
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly PostDownloadProvider _postDownloadProvider;
        private readonly ConfigProvider _configProvider;
        private readonly DiskProvider _diskProvider;

        [Inject]
        public PostDownloadScanJob(PostDownloadProvider postDownloadProvider,ConfigProvider configProvider, DiskProvider diskProvider)
        {
            _postDownloadProvider = postDownloadProvider;
            _configProvider = configProvider;
            _diskProvider = diskProvider;
        }

        public PostDownloadScanJob()
        {
        }

        public string Name
        {
            get { return "Drop folder monitor"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromMinutes(1); }
        }

        public virtual void Start(ProgressNotification notification, dynamic options)
        {
            string dropFolder;

            if (options != null && !String.IsNullOrWhiteSpace(options.Path))
                dropFolder = options.Path;

            else
                dropFolder = _configProvider.SabDropDirectory;

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

            _postDownloadProvider.ProcessDropFolder(dropFolder);
        }
    }
}