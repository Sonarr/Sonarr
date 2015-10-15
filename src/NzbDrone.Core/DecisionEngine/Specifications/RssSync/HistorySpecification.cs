using System;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.History;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.RssSync
{
    public class HistorySpecification : IDecisionEngineSpecification
    {
        private readonly IHistoryService _historyService;
        private readonly QualityUpgradableSpecification _qualityUpgradableSpecification;
        private readonly Logger _logger;

        public HistorySpecification(IHistoryService historyService,
                                           QualityUpgradableSpecification qualityUpgradableSpecification,
                                           Logger logger)
        {
            _historyService = historyService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
            _logger = logger;
        }

        public RejectionType Type { get { return RejectionType.Permanent; } }

        public virtual Decision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            if (searchCriteria != null)
            {
                _logger.Debug("Skipping history check during search");
                return Decision.Accept();
            }

            _logger.Debug("Performing history status check on report");
            foreach (var episode in subject.Episodes)
            {
                _logger.Debug("Checking current status of episode [{0}] in history", episode.Id);
                var mostRecent = _historyService.MostRecentForEpisode(episode.Id);

                if (mostRecent != null && mostRecent.EventType == HistoryEventType.Grabbed && mostRecent.DownloadId.IsNullOrWhiteSpace() && mostRecent.Date.After(DateTime.UtcNow.AddHours(-1)))
                {
                    if (!_qualityUpgradableSpecification.CutoffNotMet(subject.Series.Profile, mostRecent.Quality, subject.ParsedEpisodeInfo.Quality))
                    {
                        return Decision.Reject("Recent grab event in history already meets cutoff: {0}", mostRecent.Quality);
                    }

                    if (!_qualityUpgradableSpecification.IsUpgradable(subject.Series.Profile, mostRecent.Quality, subject.ParsedEpisodeInfo.Quality))
                    {
                        return Decision.Reject("Recent grab event in history is of equal or higher quality: {0}", mostRecent.Quality);
                    }
                }
            }

            return Decision.Accept();
        }
    }
}
