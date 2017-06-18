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
        Profile GetDefaultProfile(string name, Quality cutoff = null, params Quality[] allowed);
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
        
        public void Handle(ApplicationStartedEvent message)
        {
            if (All().Any()) return;

            _logger.Info("Setting up default quality profiles");

            AddDefaultProfile("Any", Quality.SDTV,
                Quality.SDTV,
                Quality.WEBRip480p,
                Quality.WEBDL480p,
                Quality.DVD,
                Quality.HDTV720p,
                Quality.HDTV1080p,
                Quality.WEBRip720p,
                Quality.WEBDL720p,
                Quality.WEBRip1080p,
                Quality.WEBDL1080p,
                Quality.Bluray720p,
                Quality.Bluray1080p);

            AddDefaultProfile("SD", Quality.SDTV,
                Quality.SDTV,
                Quality.WEBRip480p,
                Quality.WEBDL480p,
                Quality.DVD);

            AddDefaultProfile("HD-720p", Quality.HDTV720p,
                Quality.HDTV720p,
                Quality.WEBRip720p,
                Quality.WEBDL720p,
                Quality.Bluray720p);

            AddDefaultProfile("HD-1080p", Quality.HDTV1080p,
                Quality.HDTV1080p,
                Quality.WEBRip1080p,
                Quality.WEBDL1080p,
                Quality.Bluray1080p);

            AddDefaultProfile("Ultra-HD", Quality.HDTV2160p,
                Quality.HDTV2160p,
                Quality.WEBRip2160p,
                Quality.WEBDL2160p,
                Quality.Bluray2160p);

            AddDefaultProfile("HD - 720p/1080p", Quality.HDTV720p,
                Quality.HDTV720p,
                Quality.HDTV1080p,
                Quality.WEBRip720p,
                Quality.WEBDL720p,
                Quality.WEBRip1080p,
                Quality.WEBDL1080p,
                Quality.Bluray720p,
                Quality.Bluray1080p);
        }

        public Profile GetDefaultProfile(string name, Quality cutoff = null, params Quality[] allowed)
        {
            var groupedQualites = Quality.DefaultQualityDefinitions.GroupBy(q => q.Weight);
            var items = new List<ProfileQualityItem>();
            var groupId = 1000;
            var profileCutoff = cutoff == null ? Quality.Unknown.Id : cutoff.Id;

            foreach (var group in groupedQualites)
            {
                if (group.Count() == 1)
                {
                    var quality = group.First().Quality;

                    items.Add(new ProfileQualityItem { Quality = group.First().Quality, Allowed = allowed.Contains(quality) });
                    continue;
                }

                var groupAllowed = group.Any(g => allowed.Contains(g.Quality));

                items.Add(new ProfileQualityItem
                {
                    Id = groupId,
                    Name = group.First().GroupName,
                    Items = group.Select(g => new ProfileQualityItem
                    {
                        Quality = g.Quality,
                        Allowed = groupAllowed
                    }).ToList(),
                    Allowed = groupAllowed
                });

                if (group.Any(g => g.Quality.Id == profileCutoff))
                {
                    profileCutoff = groupId;
                }

                groupId++;
            }

            var qualityProfile = new Profile
                                 {
                                     Name = name,
                                     Cutoff = profileCutoff,
                                     Items = items
                                 };

            return qualityProfile;
        }

        private Profile AddDefaultProfile(string name, Quality cutoff, params Quality[] allowed)
        {
            var profile = GetDefaultProfile(name, cutoff, allowed);

            return Add(profile);
        }
    }
}
