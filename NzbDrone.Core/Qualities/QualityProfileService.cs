using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Qualities;
using PetaPoco;

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

    public class QualityProfileService : IQualityProfileService, IInitializable
    {
        private readonly IQualityProfileRepository _qualityProfileRepository;
        private readonly Logger _logger;

        public QualityProfileService(IQualityProfileRepository qualityProfileRepository, Logger logger)
        {
            _qualityProfileRepository = qualityProfileRepository;
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

        public void Init()
        {
            if (All().Count != 0)
                return;

            _logger.Info("Setting up default quality profiles");

            var sd = new QualityProfile { Name = "SD", Allowed = new List<Quality> { Quality.SDTV, Quality.DVD }, Cutoff = Quality.SDTV };

            var hd = new QualityProfile
            {
                Name = "HD",
                Allowed = new List<Quality> { Quality.HDTV720p, Quality.WEBDL720p, Quality.Bluray720p },
                Cutoff = Quality.HDTV720p
            };

            Add(sd);
            Add(hd);
        }
    }
}