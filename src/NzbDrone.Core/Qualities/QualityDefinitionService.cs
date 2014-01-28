using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;
using System;

namespace NzbDrone.Core.Qualities
{
    public interface IQualityDefinitionService
    {
        void Update(QualityDefinition qualityDefinition);
        List<QualityDefinition> All();
        QualityDefinition Get(Quality quality);
    }

    public class QualityDefinitionService : IQualityDefinitionService, IHandle<ApplicationStartedEvent>
    {
        private readonly IQualityDefinitionRepository _qualityDefinitionRepository;
        private readonly Logger _logger;

        public QualityDefinitionService(IQualityDefinitionRepository qualityDefinitionRepository, Logger logger)
        {
            _qualityDefinitionRepository = qualityDefinitionRepository;
            _logger = logger;
        }

        public void Update(QualityDefinition qualityDefinition)
        {
            _qualityDefinitionRepository.Update(qualityDefinition);
        }

        public List<QualityDefinition> All()
        {
            return _qualityDefinitionRepository.All().ToList();
        }
        
        public QualityDefinition Get(Quality quality)
        {
            if (quality == Quality.Unknown)
                return new QualityDefinition(Quality.Unknown);

            return _qualityDefinitionRepository.GetByQualityId((int)quality);
        }
        
        public void InsertMissingDefinitions(List<QualityDefinition> allDefinitions)
        {
            allDefinitions.OrderBy(v => v.Weight).ToList();
            var existingDefinitions = _qualityDefinitionRepository.All().OrderBy(v => v.Weight).ToList();

            // Try insert each item intelligently to merge the lists preserving the Weight the user set.
            for (int i = 0; i < allDefinitions.Count;i++)
            {
                // Skip if this definition isn't missing.
                if (existingDefinitions.Any(v => v.Quality == allDefinitions[i].Quality))
                    continue;

                int targetIndexMinimum = 0;
                for (int j = 0; j < i; j++)
                    targetIndexMinimum = Math.Max(targetIndexMinimum, existingDefinitions.FindIndex(v => v.Quality == allDefinitions[j].Quality) + 1);

                int targetIndexMaximum = existingDefinitions.Count;
                for (int j = i + 1; j < allDefinitions.Count; j++)
                {
                    var index = existingDefinitions.FindIndex(v => v.Quality == allDefinitions[j].Quality);
                    if (index != -1)
                        targetIndexMaximum = Math.Min(targetIndexMaximum, index);
                }

                // Rounded down average sounds reasonable.
                int targetIndex = (targetIndexMinimum + targetIndexMaximum) / 2;

                existingDefinitions.Insert(targetIndex, allDefinitions[i]);
            }
            
            // Update all Weights.
            List<QualityDefinition> insertList = new List<QualityDefinition>();
            List<QualityDefinition> updateList = new List<QualityDefinition>();
            for (int i = 0; i < existingDefinitions.Count; i++)
            {
                if (existingDefinitions[i].Id == 0)
                {
                    existingDefinitions[i].Weight = i + 1;
                    _qualityDefinitionRepository.Insert(existingDefinitions[i]);
                }
                else if (existingDefinitions[i].Weight != i + 1)
                {
                    existingDefinitions[i].Weight = i + 1;
                    _qualityDefinitionRepository.Update(existingDefinitions[i]);
                }
            }
        }

        public void Handle(ApplicationStartedEvent message)
        {            
            _logger.Debug("Setting up default quality config");

            InsertMissingDefinitions(Quality.DefaultQualityDefinitions.ToList());
        }
    }
}
