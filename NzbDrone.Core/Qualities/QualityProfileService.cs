using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Lifecycle;
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

        public void Handle(ApplicationStartedEvent message)
        {
            if (All().Any()) return;

            _logger.Info("Setting up default quality profiles");

            var sd = new QualityProfile
            {
                Name = "SD",
                Allowed = new List<Quality>
                              {
                                  Quality.SDTV,
                                  Quality.WEBDL480p,
                                  Quality.DVD
                              },
                Cutoff = Quality.SDTV
            };

            var hd720p = new QualityProfile
            {
                Name = "HD 720p",
                Allowed = new List<Quality>
                              {
                                  Quality.HDTV720p,
                                  Quality.WEBDL720p,
                                  Quality.Bluray720p
                              },
                Cutoff = Quality.HDTV720p
            };


            var hd1080p = new QualityProfile
            {
                Name = "HD 1080p",
                Allowed = new List<Quality>
                              {
                                  Quality.HDTV1080p,
                                  Quality.WEBDL1080p,
                                  Quality.Bluray1080p
                              },
                Cutoff = Quality.HDTV1080p
            };

            var hdAll = new QualityProfile
            {
                Name = "HD - All",
                Allowed = new List<Quality>
                              {
                                  Quality.HDTV720p,
                                  Quality.WEBDL720p,
                                  Quality.Bluray720p,
                                  Quality.HDTV1080p,
                                  Quality.WEBDL1080p,
                                  Quality.Bluray1080p
                              },
                Cutoff = Quality.HDTV720p
            };

            Add(sd);
            Add(hd720p);
            Add(hd1080p);
            Add(hdAll);
        }
    }
}