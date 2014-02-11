using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;


namespace NzbDrone.Core.Qualities
{
    public interface IQualityProfileService
    {
        QualityProfile Add(QualityProfile profile);
        void Update(QualityProfile profile);
        void Delete(int id);
        List<QualityProfile> All();
        QualityProfile Get(int id);
    }

    public class QualityProfileService : IQualityProfileService, IHandle<ApplicationStartedEvent>
    {
        private readonly IQualityProfileRepository _qualityProfileRepository;
        private readonly ISeriesService _seriesService;
        private readonly Logger _logger;

        public QualityProfileService(IQualityProfileRepository qualityProfileRepository, ISeriesService seriesService, Logger logger)
        {
            _qualityProfileRepository = qualityProfileRepository;
            _seriesService = seriesService;
            _logger = logger;
        }

        public QualityProfile Add(QualityProfile profile)
        {
            return _qualityProfileRepository.Insert(profile);
        }

        public void Update(QualityProfile profile)
        {
            _qualityProfileRepository.Update(profile);
        }

        public void Delete(int id)
        {
            if (_seriesService.GetAllSeries().Any(c => c.QualityProfileId == id))
            {
                throw new QualityProfileInUseException(id);
            }

            _qualityProfileRepository.Delete(id);
        }

        public List<QualityProfile> All()
        {
            return _qualityProfileRepository.All().ToList();
        }

        public QualityProfile Get(int id)
        {
            return _qualityProfileRepository.Get(id);
        }

        private QualityProfile AddDefaultQualityProfile(string name, Quality cutoff, params Quality[] allowed)
        {
            var items = Quality.DefaultQualityDefinitions
                            .OrderBy(v => v.Weight)
                            .Select(v => new QualityProfileItem { Quality = v.Quality, Allowed = allowed.Contains(v.Quality) })
                            .ToList();

            var qualityProfile = new QualityProfile { Name = name, Cutoff = cutoff, Items = items };

            return Add(qualityProfile);
        }

        public void Handle(ApplicationStartedEvent message)
        {
            if (All().Any()) return;

            _logger.Info("Setting up default quality profiles");

            AddDefaultQualityProfile("SD", Quality.SDTV,
                Quality.SDTV,
                Quality.WEBDL480p,
                Quality.DVD);

            AddDefaultQualityProfile("HD-720p", Quality.HDTV720p,
                Quality.HDTV720p,
                Quality.WEBDL720p,
                Quality.Bluray720p);

            AddDefaultQualityProfile("HD-1080p", Quality.HDTV1080p,
                Quality.HDTV1080p,
                Quality.WEBDL1080p,
                Quality.Bluray1080p);

            AddDefaultQualityProfile("HD - All", Quality.HDTV720p,
                Quality.HDTV720p,
                Quality.HDTV1080p,
                Quality.WEBDL720p,
                Quality.WEBDL1080p,
                Quality.Bluray720p,
                Quality.Bluray1080p);
        }
    }
}