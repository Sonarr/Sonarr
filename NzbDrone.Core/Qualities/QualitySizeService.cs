using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Qualities
{
    public interface IQualitySizeService
    {
        void Update(QualitySize qualitySize);
        List<QualitySize> All();
        QualitySize Get(int qualityId);
    }

    public class QualitySizeService : IQualitySizeService, IHandle<ApplicationStartedEvent>
    {
        private readonly IQualitySizeRepository _qualitySizeRepository;
        private readonly Logger _logger;

        public QualitySizeService(IQualitySizeRepository qualitySizeRepository, Logger logger)
        {
            _qualitySizeRepository = qualitySizeRepository;
            _logger = logger;
        }

        public virtual void Update(QualitySize qualitySize)
        {
            _qualitySizeRepository.Update(qualitySize);
        }


        public virtual List<QualitySize> All()
        {
            return _qualitySizeRepository.All().ToList();
        }

        public virtual QualitySize Get(int qualityId)
        {
            return _qualitySizeRepository.GetByQualityId(qualityId);
        }

        public void Handle(ApplicationStartedEvent message)
        {
            var existing = All();

            _logger.Debug("Setting up default quality sizes");

            foreach (var quality in Quality.All())
            {
                if (!existing.Any(s => s.QualityId == quality.Id))
                {
                    _qualitySizeRepository.Insert(new QualitySize
                    {
                        QualityId = quality.Id,
                        Name = quality.Name,
                        MinSize = 0,
                        MaxSize = 100
                    });
                }
            }
        }
    }
}
