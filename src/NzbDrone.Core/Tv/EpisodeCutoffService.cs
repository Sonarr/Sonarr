using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Profiles.Qualities;
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

        public EpisodeCutoffService(IEpisodeRepository episodeRepository, IQualityProfileService qualityProfileService, Logger logger)
        {
            _episodeRepository = episodeRepository;
            _qualityProfileService = qualityProfileService;
        }

        public PagingSpec<Episode> EpisodesWhereCutoffUnmet(PagingSpec<Episode> pagingSpec)
        {
            var qualitiesBelowCutoff = new List<QualitiesBelowCutoff>();
            var languagesBelowCutoff = new List<LanguagesBelowCutoff>();
            var profiles = _qualityProfileService.All();

            // Get all items less than the cutoff
            foreach (var profile in profiles)
            {
                var cutoff = profile.UpgradeAllowed ? profile.Cutoff : profile.FirststAllowedQuality().Id;
                var cutoffIndex = profile.GetIndex(cutoff);
                var belowCutoff = profile.Items.Take(cutoffIndex.Index).ToList();

                if (belowCutoff.Any())
                {
                    qualitiesBelowCutoff.Add(new QualitiesBelowCutoff(profile.Id, belowCutoff.SelectMany(i => i.GetQualities().Select(q => q.Id))));
                }
            }

            return _episodeRepository.EpisodesWhereCutoffUnmet(pagingSpec, qualitiesBelowCutoff, languagesBelowCutoff, false);
        }
    }
}
