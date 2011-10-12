using System;
using System.IO;
using Ninject;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers.Jobs
{
    public class PostDownloadScanJob : IJob
    {
        private readonly PostDownloadProvider _postDownloadProvider;

        [Inject]
        public PostDownloadScanJob(PostDownloadProvider postDownloadProvider)
        {
            _postDownloadProvider = postDownloadProvider;
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
            _postDownloadProvider.Start(notification);
        }
    }
}