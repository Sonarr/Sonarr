using NLog;
using NzbDrone.Core.Download;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class NotInQueueSpecification : IFetchableSpecification
    {
        private readonly DownloadProvider _downloadProvider;


        public NotInQueueSpecification(DownloadProvider downloadProvider)
        {
            _downloadProvider = downloadProvider;

        }

        public string RejectionReason
        {
            get
            {
                return "Already in download queue.";
            }
        }

        public virtual bool IsSatisfiedBy(EpisodeParseResult subject)
        {
            return !_downloadProvider.GetActiveDownloadClient().IsInQueue(subject);
        }

    }
}
