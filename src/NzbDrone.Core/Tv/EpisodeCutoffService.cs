using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Profiles;
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
        private readonly IProfileService _profileService;
        private readonly Logger _logger;

        public EpisodeCutoffService(IEpisodeRepository episodeRepository, IProfileService profileService, Logger logger)
        {
            _episodeRepository = episodeRepository;
            _profileService = profileService;
            _logger = logger;
        }

        public PagingSpec<Episode> EpisodesWhereCutoffUnmet(PagingSpec<Episode> pagingSpec)
        {
            var qualitiesBelowCutoff = new List<QualitiesBelowCutoff>();
            var profiles = _profileService.All();
            
            //Get all items less than the cutoff
            foreach (var profile in profiles)
            {
                var cutoffIndex = profile.Items.FindIndex(v => v.Quality == profile.Cutoff);
                var belowCutoff = profile.Items.Take(cutoffIndex).ToList();

                if (belowCutoff.Any())
                {
                    qualitiesBelowCutoff.Add(new QualitiesBelowCutoff(profile.Id, belowCutoff.Select(i => i.Quality.Id)));
                }
            }

            return _episodeRepository.EpisodesWhereCutoffUnmet(pagingSpec, qualitiesBelowCutoff, false);
        }
    }
}
