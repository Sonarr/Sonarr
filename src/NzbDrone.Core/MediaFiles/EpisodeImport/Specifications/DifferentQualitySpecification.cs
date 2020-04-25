using System.Linq;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Specifications
{
    public class DifferentQualitySpecification : IImportDecisionEngineSpecification
    {
        private readonly IHistoryService _historyService;
        private readonly Logger _logger;

        public DifferentQualitySpecification(IHistoryService historyService, Logger logger)
        {
            _historyService = historyService;
            _logger = logger;
        }
        public Decision IsSatisfiedBy(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            if (downloadClientItem == null)
            {
                _logger.Debug("No download client item, skipping");
                return Decision.Accept();
            }

            if (localEpisode.DownloadClientEpisodeInfo?.FullSeason == true)
            {
                _logger.Debug("Full season download, skipping");
                return Decision.Accept();
            }

            var grabbedEpisodeHistory = _historyService.FindByDownloadId(downloadClientItem.DownloadId)
                                                       .OrderByDescending(h => h.Date)
                                                       .FirstOrDefault(h => h.EventType == EpisodeHistoryEventType.Grabbed);

            if (grabbedEpisodeHistory == null)
            {
                _logger.Debug("No grabbed history for this download item, skipping");
                return Decision.Accept();
            }

            var qualityComparer = new QualityModelComparer(localEpisode.Series.QualityProfile);
            var qualityCompare = qualityComparer.Compare(localEpisode.Quality, grabbedEpisodeHistory.Quality);

            if (qualityCompare != 0)
            {
                _logger.Debug("Quality of file ({0}) does not match quality of grabbed history ({1})", localEpisode.Quality, grabbedEpisodeHistory.Quality);
                return Decision.Reject("Not an upgrade for existing episode file(s)");
            }

            return Decision.Accept();
        }
    }
}
