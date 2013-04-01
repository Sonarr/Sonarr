using NLog;
using NzbDrone.Core.Download;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class NotInQueueSpecification : IFetchableSpecification
    {
        private readonly IProvideDownloadClient _downloadClientProvider;

        public NotInQueueSpecification(IProvideDownloadClient downloadClientProvider)
        {
            _downloadClientProvider = downloadClientProvider;
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
            return !_downloadClientProvider.GetDownloadClient().IsInQueue(subject);
        }

    }
}
