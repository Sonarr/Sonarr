using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Profiles.Languages;

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
        private readonly ILanguageProfileService _languageProfileService;

        public EpisodeCutoffService(IEpisodeRepository episodeRepository, IProfileService profileService, ILanguageProfileService languageProfileService, Logger logger)
        {
            _episodeRepository = episodeRepository;
            _profileService = profileService;
            _languageProfileService = languageProfileService;
        }

        public PagingSpec<Episode> EpisodesWhereCutoffUnmet(PagingSpec<Episode> pagingSpec)
        {
            var qualitiesBelowCutoff = new List<QualitiesBelowCutoff>();
            var languagesBelowCutoff = new List<LanguagesBelowCutoff>();
            var profiles = _profileService.All();
            var languageProfiles = _languageProfileService.All();
            
            //Get all items less than the cutoff
            foreach (var profile in profiles)
            {
                var cutoffIndex = profile.GetIndex(profile.Cutoff);
                var belowCutoff = profile.Items.Take(cutoffIndex.Index).ToList();

                if (belowCutoff.Any())
                {
                    qualitiesBelowCutoff.Add(new QualitiesBelowCutoff(profile.Id, belowCutoff.SelectMany(i => i.GetQualities().Select(q => q.Id))));
                }
            }

            foreach (var profile in languageProfiles)
            {
                var languageCutoffIndex = profile.Languages.FindIndex(v => v.Language == profile.Cutoff);
                var belowLanguageCutoff = profile.Languages.Take(languageCutoffIndex).ToList();

                if (belowLanguageCutoff.Any())
                {
                    languagesBelowCutoff.Add(new LanguagesBelowCutoff(profile.Id, belowLanguageCutoff.Select(l => l.Language.Id)));
                }
            }

            return _episodeRepository.EpisodesWhereCutoffUnmet(pagingSpec, qualitiesBelowCutoff, languagesBelowCutoff, false);
        }
    }
}
