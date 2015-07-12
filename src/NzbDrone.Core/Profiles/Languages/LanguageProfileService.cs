using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Profiles.Languages
{
    public interface ILanguageProfileService
    {
        LanguageProfile Add(LanguageProfile profile);
        void Update(LanguageProfile profile);
        void Delete(int id);
        List<LanguageProfile> All();
        LanguageProfile Get(int id);
        bool Exists(int id);
    }

    public class LanguageProfileService : ILanguageProfileService, IHandle<ApplicationStartedEvent>
    {
        private readonly ILanguageProfileRepository _profileRepository;
        private readonly ISeriesService _seriesService;
        private readonly Logger _logger;

        public LanguageProfileService(ILanguageProfileRepository profileRepository, ISeriesService seriesService, Logger logger)
        {
            _profileRepository = profileRepository;
            _seriesService = seriesService;
            _logger = logger;
        }

        public LanguageProfile Add(LanguageProfile profile)
        {
            return _profileRepository.Insert(profile);
        }

        public void Update(LanguageProfile profile)
        {
            _profileRepository.Update(profile);
        }

        public void Delete(int id)
        {
            if (_seriesService.GetAllSeries().Any(c => c.LanguageProfileId == id))
            {
                throw new LanguageProfileInUseException(id);
            }

            _profileRepository.Delete(id);
        }

        public List<LanguageProfile> All()
        {
            return _profileRepository.All().ToList();
        }

        public LanguageProfile Get(int id)
        {
            return _profileRepository.Get(id);
        }

        public bool Exists(int id)
        {
            return _profileRepository.Exists(id);
        }

        private LanguageProfile AddDefaultProfile(string name, Language cutoff, params Language[] allowed)
        {
            var languages = Language.All
                                    .OrderByDescending(l => l.Name)
                                    .Select(v => new ProfileLanguageItem { Language = v, Allowed = allowed.Contains(v) })
                                    .ToList();

            var profile = new LanguageProfile
            {
                Name = name, 
                Cutoff = cutoff, 
                Languages = languages, 
            };

            return Add(profile);
        }

        public void Handle(ApplicationStartedEvent message)
        {
            if (All().Any()) return;

            _logger.Info("Setting up default language profiles");

            AddDefaultProfile("English", Language.English, Language.English);
        }
    }
}