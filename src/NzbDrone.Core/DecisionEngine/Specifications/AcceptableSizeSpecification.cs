using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using System.Collections.Generic;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class AcceptableSizeSpecification : IDecisionEngineSpecification
    {
        private readonly IQualityDefinitionService _qualityDefinitionService;
        private readonly IEpisodeService _episodeService;
        private readonly Logger _logger;

        public AcceptableSizeSpecification(IQualityDefinitionService qualityDefinitionService, IEpisodeService episodeService, Logger logger)
        {
            _qualityDefinitionService = qualityDefinitionService;
            _episodeService = episodeService;
            _logger = logger;
        }

        public RejectionType Type => RejectionType.Permanent;

        public Decision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            _logger.Debug("Beginning size check for: {0}", subject);

            var quality = subject.ParsedEpisodeInfo.Quality.Quality;

            if (subject.ParsedEpisodeInfo.Special)
            {
                _logger.Debug("Special release found, skipping size check.");
                return Decision.Accept();
            }

            if (subject.Release.Size == 0)
            {
                _logger.Debug("Release has unknown size, skipping size check.");
                return Decision.Accept();
            }

            var qualityDefinition = _qualityDefinitionService.Get(quality);
            if (qualityDefinition.MinSize.HasValue)
            {
                var minSize = qualityDefinition.MinSize.Value.Megabytes();

                //Multiply maxSize by Series.Runtime
                minSize = minSize * subject.Series.Runtime * subject.Episodes.Count;

                //If the parsed size is smaller than minSize we don't want it
                if (subject.Release.Size < minSize)
                {
                    var runtimeMessage = subject.Episodes.Count == 1 ? $"{subject.Series.Runtime}min" : $"{subject.Episodes.Count}x {subject.Series.Runtime}min";

                    _logger.Debug("Item: {0}, Size: {1} is smaller than minimum allowed size ({2} bytes for {3}), rejecting.", subject, subject.Release.Size, minSize, runtimeMessage);
                    return Decision.Reject("{0} is smaller than minimum allowed {1} (for {2})", subject.Release.Size.SizeSuffix(), minSize.SizeSuffix(), runtimeMessage);
                }
            }
            if (!qualityDefinition.MaxSize.HasValue || qualityDefinition.MaxSize.Value == 0)
            {
                _logger.Debug("Max size is unlimited - skipping check.");
            }
            else
            {
                var maxSize = qualityDefinition.MaxSize.Value.Megabytes();

                //Multiply maxSize by Series.Runtime
                maxSize = maxSize * subject.Series.Runtime * subject.Episodes.Count;

                if (subject.Episodes.Count == 1)
                {
                    Episode episode = subject.Episodes.First();
                    List<Episode> seasonEpisodes;

                    var seasonSearchCriteria = searchCriteria as SeasonSearchCriteria;
                    if (seasonSearchCriteria != null && !seasonSearchCriteria.Series.UseSceneNumbering && seasonSearchCriteria.Episodes.Any(v => v.Id == episode.Id))
                    {
                        seasonEpisodes = (searchCriteria as SeasonSearchCriteria).Episodes;
                    }
                    else
                    {
                        seasonEpisodes = _episodeService.GetEpisodesBySeason(episode.SeriesId, episode.SeasonNumber);
                    }

                    //Ensure that this is either the first episode
                    //or is the last episode in a season that has 10 or more episodes
                    if (seasonEpisodes.First().Id == episode.Id || (seasonEpisodes.Count() >= 10 && seasonEpisodes.Last().Id == episode.Id))
                    {
                        _logger.Debug("Possible double episode, doubling allowed size.");
                        maxSize = maxSize * 2;
                    }
                }

                //If the parsed size is greater than maxSize we don't want it
                if (subject.Release.Size > maxSize)
                {
                    var runtimeMessage = subject.Episodes.Count == 1 ? $"{subject.Series.Runtime}min" : $"{subject.Episodes.Count}x {subject.Series.Runtime}min";

                    _logger.Debug("Item: {0}, Size: {1} is greater than maximum allowed size ({2} for {3}), rejecting.", subject, subject.Release.Size, maxSize, runtimeMessage);
                    return Decision.Reject("{0} is larger than maximum allowed {1} (for {2})", subject.Release.Size.SizeSuffix(), maxSize.SizeSuffix(), runtimeMessage);
                }
            }

            _logger.Debug("Item: {0}, meets size constraints.", subject);
            return Decision.Accept();
        }
    }
}
