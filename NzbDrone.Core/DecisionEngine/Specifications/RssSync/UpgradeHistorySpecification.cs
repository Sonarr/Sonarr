using NLog;
using NzbDrone.Core.History;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.RssSync
{
    public class UpgradeHistorySpecification : IDecisionEngineSpecification
    {
        private readonly IHistoryService _historyService;
        private readonly QualityUpgradableSpecification _qualityUpgradableSpecification;
        private readonly Logger _logger;

        public UpgradeHistorySpecification(IHistoryService historyService, QualityUpgradableSpecification qualityUpgradableSpecification, Logger logger)
        {
            _historyService = historyService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
            _logger = logger;
        }

        public string RejectionReason
        {
            get
            {
                return "Existing file in history is of equal or higher quality";
            }
        }

        public virtual bool IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            if (searchCriteria != null)
            {
                _logger.Trace("Skipping history check during search");
                return true;
            }

            foreach (var episode in subject.Episodes)
            {
                var bestQualityInHistory = _historyService.GetBestQualityInHistory(episode.Id);
                if (bestQualityInHistory != null)
                {
                    _logger.Trace("Comparing history quality with report. History is {0}", bestQualityInHistory);
                    if (!_qualityUpgradableSpecification.IsUpgradable(bestQualityInHistory, subject.ParsedEpisodeInfo.Quality))
                        return false;
                }
            }

            return true;
        }
    }
}
