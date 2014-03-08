using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Tv
{
    public interface IEpisodeCutoffService
    {
        PagingSpec<Episode> EpisodesWhereCutoffUnmet(PagingSpec<Episode> pagingSpec);
    }

    public class EpisodeCutoffService : IEpisodeCutoffService
    {
        private readonly IEpisodeRepository _episodeRepository;
        private readonly IQualityProfileService _qualityProfileService;
        private readonly Logger _logger;

        public EpisodeCutoffService(IEpisodeRepository episodeRepository, IQualityProfileService qualityProfileService, Logger logger)
        {
            _episodeRepository = episodeRepository;
            _qualityProfileService = qualityProfileService;
            _logger = logger;
        }

        public PagingSpec<Episode> EpisodesWhereCutoffUnmet(PagingSpec<Episode> pagingSpec)
        {
            var qualitiesBelowCutoff = new List<QualitiesBelowCutoff>();
            var qualityProfiles = _qualityProfileService.All();
            
            //Get all items less than the cutoff
            foreach (var qualityProfile in qualityProfiles)
            {
                var cutoffIndex = qualityProfile.Items.FindIndex(v => v.Quality == qualityProfile.Cutoff);
                var belowCutoff = qualityProfile.Items.Take(cutoffIndex).ToList();

                if (belowCutoff.Any())
                {
                    qualitiesBelowCutoff.Add(new QualitiesBelowCutoff(qualityProfile.Id, belowCutoff.Select(i => i.Quality.Id)));
                }
            }

            return _episodeRepository.EpisodesWhereCutoffUnmet(pagingSpec, qualitiesBelowCutoff, false);
        }
    }
}
