using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.Search
{
    public class TorrentSeedingSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public TorrentSeedingSpecification(Logger logger)
        {
            _logger = logger;
        }

        public RejectionType Type => RejectionType.Permanent;


        public Decision IsSatisfiedBy(RemoteEpisode remoteEpisode, SearchCriteriaBase searchCriteria)
        {
            var torrentInfo = remoteEpisode.Release as TorrentInfo;

            if (torrentInfo == null)
            {
                return Decision.Accept();
            }

            if (torrentInfo.Seeders != null && torrentInfo.Seeders < 1)
            {
                _logger.Debug("Not enough seeders. ({0})", torrentInfo.Seeders);
                return Decision.Reject("Not enough seeders. ({0})", torrentInfo.Seeders);
            }

            return Decision.Accept();
        }
    }
}