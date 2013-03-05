using System.Linq;
using System;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Jobs.Framework;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Jobs
{
    public class PostDownloadScanJob : IJob
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly PostDownloadProvider _postDownloadProvider;
        private readonly IConfigService _configService;
        private readonly DiskProvider _diskProvider;

        public PostDownloadScanJob(PostDownloadProvider postDownloadProvider,IConfigService configService, DiskProvider diskProvider)
        {
            _postDownloadProvider = postDownloadProvider;
            _configService = configService;
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
                dropFolder = _configService.DownloadClientTvDirectory;

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