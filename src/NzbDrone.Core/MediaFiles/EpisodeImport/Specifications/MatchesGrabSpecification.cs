using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Specifications
{
    public class MatchesGrabSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;
        private readonly IParsingService _parsingService;
        private readonly IHistoryService _historyService;

        public MatchesGrabSpecification(IParsingService parsingService, IHistoryService historyService, Logger logger)
        {
            _logger = logger;
            _parsingService = parsingService;
            _historyService = historyService;
        }

        public Decision IsSatisfiedBy(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            if (localEpisode.ExistingFile)
            {
                return Decision.Accept();
            }

            if (downloadClientItem == null)
            {
                return Decision.Accept();
            }

            var grabbedHistory = _historyService.FindByDownloadId(downloadClientItem.DownloadId)
                .Where(h => h.EventType == EpisodeHistoryEventType.Grabbed)
                .ToList();

            if (grabbedHistory.Empty())
            {
                return Decision.Accept();
            }

            var unexpected = localEpisode.Episodes.Where(e => grabbedHistory.All(o => o.EpisodeId != e.Id)).ToList();

            if (unexpected.Any())
            {
                _logger.Debug("Unexpected episode(s) in file: {0}", FormatEpisode(unexpected));

                if (unexpected.Count == 1)
                {
                    return Decision.Reject("Episode {0} was not found in the grabbed release: {1}", FormatEpisode(unexpected), grabbedHistory.First().SourceTitle);
                }

                return Decision.Reject("Episodes {0} were not found in the grabbed release: {1}", FormatEpisode(unexpected), grabbedHistory.First().SourceTitle);
            }

            return Decision.Accept();
        }

        private string FormatEpisode(List<Episode> episodes)
        {
            return string.Join(", ", episodes.Select(e => $"{e.SeasonNumber}x{e.EpisodeNumber:00}"));
        }
    }
}
