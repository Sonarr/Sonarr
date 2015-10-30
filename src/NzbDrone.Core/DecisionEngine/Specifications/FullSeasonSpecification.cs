using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Common.Extensions;
using System.Linq;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class FullSeasonSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;
        private readonly IEpisodeService _episodeService;

        public FullSeasonSpecification(Logger logger, IEpisodeService episodeService)
        {
            _logger = logger;
            _episodeService = episodeService;
        }

        public RejectionType Type { get { return RejectionType.Permanent; } }

        public virtual Decision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            if (subject.ParsedEpisodeInfo.FullSeason)
            {
                _logger.Debug("Checking if report is a full season. {0}", subject.ParsedEpisodeInfo.SeasonNumber);
                if (subject.Episodes.Any(e => e.AirDateUtc.HasValue && e.AirDateUtc.Value.After(System.DateTime.UtcNow.AddDays(-7))))
                {
                    _logger.Debug("Report Full Season {0} rejected. At least one episode younger than one week.", subject.ParsedEpisodeInfo.SeasonNumber);
                    return Decision.Reject("Full season {0} have pending episodes to be aired.", subject.ParsedEpisodeInfo.SeasonNumber);
                }

                if (_episodeService.EpisodesBetweenDates(System.DateTime.UtcNow.AddDays(-1), System.DateTime.UtcNow, false).Any(e => e.SeriesId == subject.Series.Id))
                {
                    _logger.Debug("Report Full Season {0} rejected. An episode for that show has been aired in the last 24 hours.", subject.ParsedEpisodeInfo.SeasonNumber);
                    return Decision.Reject("Not a full season ({0}). Episode aired today.", subject.ParsedEpisodeInfo.SeasonNumber);
                }
            }

            return Decision.Accept();
        }
    }
}
