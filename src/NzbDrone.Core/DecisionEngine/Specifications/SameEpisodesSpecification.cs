using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class SameEpisodesSpecification
    {
        private readonly IEpisodeService _episodeService;

        public SameEpisodesSpecification(IEpisodeService episodeService)
        {
            _episodeService = episodeService;
        }

        public bool IsSatisfiedBy(List<Episode> episodes)
        {
            var episodeIds = episodes.SelectList(e => e.Id);
            var episodeFileIds = episodes.Where(c => c.EpisodeFileId != 0).Select(c => c.EpisodeFileId).Distinct();

            foreach (var episodeFileId in episodeFileIds)
            {
                var episodesInFile = _episodeService.GetEpisodesByFileId(episodeFileId);

                if (episodesInFile.Select(e => e.Id).Except(episodeIds).Any())
                {
                    return false;
                }
            }

            return true;
        }
    }
}
