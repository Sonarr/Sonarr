using NLog;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.Sabnzbd;
using NzbDrone.Core.History;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.RssSync
{
    public class HistorySpecification : IDecisionEngineSpecification
    {
        private readonly IHistoryService _historyService;
        private readonly QualityUpgradableSpecification _qualityUpgradableSpecification;
        private readonly IProvideDownloadClient _downloadClientProvider;
        private readonly Logger _logger;

        public HistorySpecification(IHistoryService historyService,
                                           QualityUpgradableSpecification qualityUpgradableSpecification,
                                           IProvideDownloadClient downloadClientProvider,
                                           Logger logger)
        {
            _historyService = historyService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
            _downloadClientProvider = downloadClientProvider;
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

            if (_downloadClientProvider.GetDownloadClient().GetType() == typeof (SabnzbdClient))
            {
                _logger.Trace("Performing history status check on report");
                foreach (var episode in subject.Episodes)
                {
                    _logger.Trace("Checking current status of episode [{0}] in history", episode.Id);
                    var mostRecent = _historyService.MostRecentForEpisode(episode.Id);

                    if (mostRecent != null && mostRecent.EventType == HistoryEventType.Grabbed)
                    {
                        return false;
                    }
                }
                return true;
            }

            foreach (var episode in subject.Episodes)
            {
                var bestQualityInHistory = _historyService.GetBestQualityInHistory(subject.Series.QualityProfile, episode.Id);
                if (bestQualityInHistory != null)
                {
                    _logger.Trace("Comparing history quality with report. History is {0}", bestQualityInHistory);
                    if (!_qualityUpgradableSpecification.IsUpgradable(subject.Series.QualityProfile, bestQualityInHistory, subject.ParsedEpisodeInfo.Quality))
                        return false;
                }
            }

            return true;
        }
    }
}
