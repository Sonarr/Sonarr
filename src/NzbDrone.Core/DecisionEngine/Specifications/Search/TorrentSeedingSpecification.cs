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

        public string RejectionReason
        {
            get
            {
                return "Not enough Torrent seeders";
            }
        }

        public RejectionType Type
        {
            get
            {
                return RejectionType.Permanent;
            }
        }

        public bool IsSatisfiedBy(RemoteEpisode remoteEpisode, SearchCriteriaBase searchCriteria)
        {
            var torrentInfo = remoteEpisode.Release as TorrentInfo;

            if (torrentInfo == null)
            {
                return true;
            }

            if (torrentInfo.Seeds != null && torrentInfo.Seeds < 1)
            {
                _logger.Debug("Only {0} seeders, skipping.", torrentInfo.Seeds);
                return false;
            }

            return true;
        }
    }
}