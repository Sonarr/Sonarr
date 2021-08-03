using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Profiles.Qualities
{
    public interface IQualityProfileService
    {
        QualityProfile Add(QualityProfile profile);
        void Update(QualityProfile profile);
        void Delete(int id);
        List<QualityProfile> All();
        QualityProfile Get(int id);
        bool Exists(int id);
        QualityProfile GetDefaultProfile(string name, Quality cutoff = null, params Quality[] allowed);
    }

    public class QualityProfileService : IQualityProfileService, IHandle<ApplicationStartedEvent>
    {
        private readonly IProfileRepository _profileRepository;
        private readonly IImportListFactory _importListFactory;
        private readonly ISeriesService _seriesService;
        private readonly Logger _logger;

        public QualityProfileService(IProfileRepository profileRepository, IImportListFactory importListFactory, ISeriesService seriesService, Logger logger)
        {
            _profileRepository = profileRepository;
            _importListFactory = importListFactory;
            _seriesService = seriesService;
            _logger = logger;
        }

        public QualityProfile Add(QualityProfile profile)
        {
            return _profileRepository.Insert(profile);
        }

        public void Update(QualityProfile profile)
        {
            _profileRepository.Update(profile);
        }

        public void Delete(int id)
        {
            if (_seriesService.GetAllSeries().Any(c => c.QualityProfileId == id) || _importListFactory.All().Any(c => c.QualityProfileId == id))
            {
                var profile = _profileRepository.Get(id);
                throw new QualityProfileInUseException(profile.Name);
            }

            _profileRepository.Delete(id);
        }

        public List<QualityProfile> All()
        {
            return _profileRepository.All().ToList();
        }

        public QualityProfile Get(int id)
        {
            return _profileRepository.Get(id);
        }

        public bool Exists(int id)
        {
            return _profileRepository.Exists(id);
        }

        public void Handle(ApplicationStartedEvent message)
        {
            if (All().Any())
            {
                return;
            }

            _logger.Info("Setting up default quality profiles");

            AddDefaultProfile("Any",
                Quality.SDTV,
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

            AddDefaultProfile("SD",
                Quality.SDTV,
                Quality.SDTV,
                Quality.WEBRip480p,
                Quality.WEBDL480p,
                Quality.DVD);

            AddDefaultProfile("HD-720p",
                Quality.HDTV720p,
                Quality.HDTV720p,
                Quality.WEBRip720p,
                Quality.WEBDL720p,
                Quality.Bluray720p);

            AddDefaultProfile("HD-1080p",
                Quality.HDTV1080p,
                Quality.HDTV1080p,
                Quality.WEBRip1080p,
                Quality.WEBDL1080p,
                Quality.Bluray1080p);

            AddDefaultProfile("Ultra-HD",
                Quality.HDTV2160p,
                Quality.HDTV2160p,
                Quality.WEBRip2160p,
                Quality.WEBDL2160p,
                Quality.Bluray2160p);

            AddDefaultProfile("HD - 720p/1080p",
                Quality.HDTV720p,
                Quality.HDTV720p,
                Quality.HDTV1080p,
                Quality.WEBRip720p,
                Quality.WEBDL720p,
                Quality.WEBRip1080p,
                Quality.WEBDL1080p,
                Quality.Bluray720p,
                Quality.Bluray1080p);
        }

        public QualityProfile GetDefaultProfile(string name, Quality cutoff = null, params Quality[] allowed)
        {
            var groupedQualites = Quality.DefaultQualityDefinitions.GroupBy(q => q.Weight);
            var items = new List<QualityProfileQualityItem>();
            var groupId = 1000;
            var profileCutoff = cutoff == null ? Quality.Unknown.Id : cutoff.Id;

            foreach (var group in groupedQualites)
            {
                if (group.Count() == 1)
                {
                    var quality = group.First().Quality;

                    items.Add(new QualityProfileQualityItem { Quality = group.First().Quality, Allowed = allowed.Contains(quality) });
                    continue;
                }

                var groupAllowed = group.Any(g => allowed.Contains(g.Quality));

                items.Add(new QualityProfileQualityItem
                {
                    Id = groupId,
                    Name = group.First().GroupName,
                    Items = group.Select(g => new QualityProfileQualityItem
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

            var qualityProfile = new QualityProfile
                                 {
                                     Name = name,
                                     Cutoff = profileCutoff,
                                     Items = items
                                 };

            return qualityProfile;
        }

        private QualityProfile AddDefaultProfile(string name, Quality cutoff, params Quality[] allowed)
        {
            var profile = GetDefaultProfile(name, cutoff, allowed);

            return Add(profile);
        }
    }
}
