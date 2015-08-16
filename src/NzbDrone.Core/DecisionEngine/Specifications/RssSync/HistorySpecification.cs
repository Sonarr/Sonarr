using System.Linq;
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

        public RejectionType Type { get { return RejectionType.Permanent; } }

        public virtual Decision IsSatisfiedBy(RemoteItem subject, SearchCriteriaBase searchCriteria)
        {
            if (searchCriteria != null)
            {
                _logger.Debug("Skipping history check during search");
                return Decision.Accept();
            }

            var downloadClients = _downloadClientProvider.GetDownloadClients();

            foreach (var downloadClient in downloadClients.OfType<Sabnzbd>())
            {
                _logger.Debug("Performing history status check on report");
                if (subject is RemoteEpisode)
                {
                    foreach (var episode in (subject as RemoteEpisode).Episodes)
                    {
                        _logger.Debug("Checking current status of episode [{0}] in history", episode.Id);
                        var mostRecent = _historyService.MostRecentForEpisode(episode.Id);

                        if (mostRecent != null && mostRecent.EventType == HistoryEventType.Grabbed)
                        {
                            _logger.Debug("Latest history item is downloading, rejecting.");
                            return Decision.Reject("Download has not been imported yet");
                        }
                    }
                }

                if (subject is RemoteMovie)
                {
                    _logger.Debug("Checking current status of movie [{0}] in history", subject.Media);
                    var mostRecent = _historyService.MostRecentForMovie(subject.Media.Id);
                    if (mostRecent != null && mostRecent.EventType == HistoryEventType.Grabbed)
                    {
                        _logger.Debug("Latest history item is downloading, rejecting.");
                        return Decision.Reject("Download has not been imported yet");
                    }

                }
                return Decision.Accept();
            }

            if (subject is RemoteEpisode)
            {
                foreach (var episode in (subject as RemoteEpisode).Episodes)
                {
                    var bestQualityInHistory = _historyService.GetBestEpisodeQualityInHistory(subject.Media.Profile, episode.Id);
                    if (bestQualityInHistory != null)
                    {
                        _logger.Debug("Comparing history quality with report. History is {0}", bestQualityInHistory);

                        if (!_qualityUpgradableSpecification.IsUpgradable(subject.Media.Profile, bestQualityInHistory, subject.ParsedInfo.Quality))
                        {
                            return Decision.Reject("Existing file in history is of equal or higher quality: {0}", bestQualityInHistory);
                        }
                    }
                }
            }

            if (subject is RemoteMovie)
            {
                var bestQualityInHistory = _historyService.GetBestMovieQualityInHistory(subject.Media.Profile, subject.Media.Id);
                if (bestQualityInHistory != null)
                {
                    _logger.Debug("Comparing history quality with report. History is {0}", bestQualityInHistory);

                    if (!_qualityUpgradableSpecification.IsUpgradable(subject.Media.Profile, bestQualityInHistory, subject.ParsedInfo.Quality))
                    {
                        return Decision.Reject("Existing file in history is of equal or higher quality: {0}", bestQualityInHistory);
                    }
                }
            }

            return Decision.Accept();
        }
    }
}
