using System;
using System.Linq;
using NLog;
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

        public String RejectionReason
        {
            get { return "File size too big or small"; }
        }

        public RejectionType Type { get { return RejectionType.Permanent; } }

        public  bool IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            _logger.Debug("Beginning size check for: {0}", subject);

            var quality = subject.ParsedEpisodeInfo.Quality.Quality;

            if (quality == Quality.RAWHD)
            {
                _logger.Debug("Raw-HD release found, skipping size check.");
                return true;
            }

            if (quality == Quality.Unknown)
            {
                _logger.Debug("Unknown quality. skipping size check.");
                return false;
            }

            var qualityDefinition = _qualityDefinitionService.Get(quality);
            var minSize = qualityDefinition.MinSize.Megabytes();

            //Multiply maxSize by Series.Runtime
            minSize = minSize * subject.Series.Runtime * subject.Episodes.Count;

            //If the parsed size is smaller than minSize we don't want it
            if (subject.Release.Size < minSize)
            {
                _logger.Debug("Item: {0}, Size: {1} is smaller than minimum allowed size ({2}), rejecting.", subject, subject.Release.Size, minSize);
                return false;
            }
            if (qualityDefinition.MaxSize == 0)
            {
                _logger.Debug("Max size is 0 (unlimited) - skipping check.");
            }
            else
            {
                var maxSize = qualityDefinition.MaxSize.Megabytes();

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
                        maxSize = maxSize * 2;
                    }
                }

                //If the parsed size is greater than maxSize we don't want it
                if (subject.Release.Size > maxSize)
                {
                    _logger.Debug("Item: {0}, Size: {1} is greater than maximum allowed size ({2}), rejecting.", subject, subject.Release.Size, maxSize);
                    return false;
                }
            }
            _logger.Debug("Item: {0}, meets size constraints.", subject);
            return true;
        }
    }
}
