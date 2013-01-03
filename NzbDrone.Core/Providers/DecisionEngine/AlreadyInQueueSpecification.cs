using System.Linq;
using NLog;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.Providers.DecisionEngine
{
    public class AlreadyInQueueSpecification
    {
        private readonly DownloadProvider _downloadProvider;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public AlreadyInQueueSpecification(DownloadProvider downloadProvider)
        {
            _downloadProvider = downloadProvider;

        }

        public AlreadyInQueueSpecification()
        {
            
        }

        public virtual bool IsSatisfiedBy(EpisodeParseResult subject)
        {
            return _downloadProvider.GetActiveDownloadClient().IsInQueue(subject);
        }
       
    }
}
