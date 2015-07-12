using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Profiles.Qualities
{
    public interface IProfileService
    {
        Profile Add(Profile profile);
        void Update(Profile profile);
        void Delete(int id);
        List<Profile> All();
        Profile Get(int id);
        bool Exists(int id);
    }

    public class ProfileService : IProfileService, IHandle<ApplicationStartedEvent>
    {
        private readonly IProfileRepository _profileRepository;
        private readonly ISeriesService _seriesService;
        private readonly Logger _logger;

        public ProfileService(IProfileRepository profileRepository, ISeriesService seriesService, Logger logger)
        {
            _profileRepository = profileRepository;
            _seriesService = seriesService;
            _logger = logger;
        }

        public Profile Add(Profile profile)
        {
            return _profileRepository.Insert(profile);
        }

        public void Update(Profile profile)
        {
            _profileRepository.Update(profile);
        }

        public void Delete(int id)
        {
            if (_seriesService.GetAllSeries().Any(c => c.ProfileId == id))
            {
                var profile = _profileRepository.Get(id);
                throw new ProfileInUseException(profile.Name);
            }

            _profileRepository.Delete(id);
        }

        public List<Profile> All()
        {
            return _profileRepository.All().ToList();
        }

        public Profile Get(int id)
        {
            return _profileRepository.Get(id);
        }

        public bool Exists(int id)
        {
            return _profileRepository.Exists(id);
        }

        private Profile AddDefaultProfile(string name, Quality cutoff, params Quality[] allowed)
        {
            var items = Quality.DefaultQualityDefinitions
                            .OrderBy(v => v.Weight)
                            .Select(v => new ProfileQualityItem { Quality = v.Quality, Allowed = allowed.Contains(v.Quality) })
                            .ToList();

            var profile = new Profile { Name = name, 
                Cutoff = cutoff, 
                Items = items, 
            };

            return Add(profile);
        }

        public void Handle(ApplicationStartedEvent message)
        {
            if (All().Any()) return;

            _logger.Info("Setting up default quality profiles");

            AddDefaultProfile("Any", Quality.SDTV,
                Quality.SDTV,
                Quality.WEBDL480p,
                Quality.DVD,
                Quality.HDTV720p,
                Quality.HDTV1080p,
                Quality.WEBDL720p,
                Quality.WEBDL1080p,
                Quality.Bluray720p,
                Quality.Bluray1080p);

            AddDefaultProfile("SD", Quality.SDTV,
                Quality.SDTV,
                Quality.WEBDL480p,
                Quality.DVD);

            AddDefaultProfile("HD-720p", Quality.HDTV720p,
                Quality.HDTV720p,
                Quality.WEBDL720p,
                Quality.Bluray720p);

            AddDefaultProfile("HD-1080p", Quality.HDTV1080p,
                Quality.HDTV1080p,
                Quality.WEBDL1080p,
                Quality.Bluray1080p);

            AddDefaultProfile("Ultra-HD", Quality.HDTV2160p,
                Quality.HDTV2160p,
                Quality.WEBDL2160p,
                Quality.Bluray2160p);

            AddDefaultProfile("HD - 720p/1080p", Quality.HDTV720p,
                Quality.HDTV720p,
                Quality.HDTV1080p,
                Quality.WEBDL720p,
                Quality.WEBDL1080p,
                Quality.Bluray720p,
                Quality.Bluray1080p);
        }
    }
}
