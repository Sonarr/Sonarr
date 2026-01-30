using System;
using System.Linq;
using NLog;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.RssSync
{
    public class AirDateSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public AirDateSpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual DownloadSpecDecision IsSatisfiedBy(RemoteEpisode subject, ReleaseDecisionInformation information)
        {
            // Allow user-invoked searches to bypass air date check (user explicitly wants to search)
            if (information.SearchCriteria is { UserInvokedSearch: true })
            {
                _logger.Debug("Skipping air date check for user invoked search");
                return DownloadSpecDecision.Accept();
            }

            // Check if any episode hasn't aired yet
            var unairedEpisodes = subject.Episodes
                .Where(e => !e.AirDateUtc.HasValue || e.AirDateUtc.Value.After(DateTime.UtcNow))
                .ToList();

            if (unairedEpisodes.Any())
            {
                if (unairedEpisodes.Count == 1)
                {
                    var episode = unairedEpisodes.First();
                    if (!episode.AirDateUtc.HasValue)
                    {
                        _logger.Debug("Episode {0}x{1} ({2}) rejected. Air date is not set.", episode.SeasonNumber, episode.EpisodeNumber, episode.Title);
                        return DownloadSpecDecision.Reject(DownloadRejectionReason.EpisodeNotAired, $"Episode {episode.SeasonNumber}x{episode.EpisodeNumber} ({episode.Title}) has not aired yet (air date not set)");
                    }
                    else
                    {
                        _logger.Debug("Episode {0}x{1} ({2}) rejected. Air date is {3}, which is in the future.", episode.SeasonNumber, episode.EpisodeNumber, episode.Title, episode.AirDateUtc.Value);
                        return DownloadSpecDecision.Reject(DownloadRejectionReason.EpisodeNotAired, $"Episode {episode.SeasonNumber}x{episode.EpisodeNumber} ({episode.Title}) has not aired yet (airs on {episode.AirDateUtc.Value:yyyy-MM-dd})");
                    }
                }
                else
                {
                    var episodeList = string.Join(", ", unairedEpisodes.Select(e => $"{e.SeasonNumber}x{e.EpisodeNumber}"));
                    _logger.Debug("Release rejected. {0} episodes have not aired yet: {1}", unairedEpisodes.Count, episodeList);
                    return DownloadSpecDecision.Reject(DownloadRejectionReason.EpisodeNotAired, $"Release contains {unairedEpisodes.Count} episode(s) that have not aired yet: {episodeList}");
                }
            }

            return DownloadSpecDecision.Accept();
        }
    }
}

