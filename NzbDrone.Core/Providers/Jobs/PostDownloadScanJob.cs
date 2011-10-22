using Ninject;
using NzbDrone.Core.Model.Notification;

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
            _postDownloadProvider.ScanDropFolder(notification);
        }
    }
}