using NLog;
using Workarr.DecisionEngine;
using Workarr.Download;
using Workarr.Extensions;
using Workarr.History;
using Workarr.Parser.Model;

namespace Workarr.MediaFiles.EpisodeImport.Specifications
{
    public class AlreadyImportedSpecification : IImportDecisionEngineSpecification
    {
        private readonly IHistoryService _historyService;
        private readonly Logger _logger;

        public AlreadyImportedSpecification(IHistoryService historyService,
                                            Logger logger)
        {
            _historyService = historyService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Database;

        public ImportSpecDecision IsSatisfiedBy(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            if (downloadClientItem == null)
            {
                _logger.Debug("No download client information is available, skipping");
                return ImportSpecDecision.Accept();
            }

            foreach (var episode in localEpisode.Episodes)
            {
                if (!episode.HasFile)
                {
                    _logger.Debug("Skipping already imported check for episode without file");
                    continue;
                }

                var episodeHistory = _historyService.FindByEpisodeId(episode.Id);
                var lastImported = episodeHistory.FirstOrDefault(h =>
                    h.DownloadId == downloadClientItem.DownloadId &&
                    h.EventType == EpisodeHistoryEventType.DownloadFolderImported);
                var lastGrabbed = episodeHistory.FirstOrDefault(h =>
                    h.DownloadId == downloadClientItem.DownloadId && h.EventType == EpisodeHistoryEventType.Grabbed);

                if (lastImported == null)
                {
                    _logger.Trace("Episode file has not been imported");
                    continue;
                }

                if (lastGrabbed != null)
                {
                    // If the release was grabbed again after importing don't reject it
                    if (DateTimeExtensions.After(lastGrabbed.Date, lastImported.Date))
                    {
                        _logger.Trace("Episode file was grabbed again after importing");
                        continue;
                    }

                    // If the release was imported after the last grab reject it
                    if (DateTimeExtensions.After(lastImported.Date, lastGrabbed.Date))
                    {
                        _logger.Debug<DateTime>("Episode file previously imported at {0}", lastImported.Date);
                        return ImportSpecDecision.Reject(ImportRejectionReason.EpisodeAlreadyImported, "Episode file already imported at {0}", lastImported.Date.ToLocalTime());
                    }
                }
                else
                {
                    _logger.Debug<DateTime>("Episode file previously imported at {0}", lastImported.Date);
                    return ImportSpecDecision.Reject(ImportRejectionReason.EpisodeAlreadyImported, "Episode file already imported at {0}", lastImported.Date.ToLocalTime());
                }
            }

            return ImportSpecDecision.Accept();
        }
    }
}
