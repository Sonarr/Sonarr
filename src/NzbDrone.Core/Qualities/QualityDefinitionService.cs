using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;
using System;
using NzbDrone.Common.Cache;

namespace NzbDrone.Core.Qualities
{
    public interface IQualityDefinitionService
    {
        void Update(QualityDefinition qualityDefinition);
        void UpdateMany(List<QualityDefinition> qualityDefinitions);
        List<QualityDefinition> All();
        QualityDefinition GetById(int id);
        QualityDefinition Get(Quality quality);
    }

    public class QualityDefinitionService : IQualityDefinitionService, IHandle<ApplicationStartedEvent>
    {
        private readonly IQualityDefinitionRepository _repo;
        private readonly ICached<Dictionary<Quality, QualityDefinition>> _cache;
        private readonly Logger _logger;

        public QualityDefinitionService(IQualityDefinitionRepository repo, ICacheManager cacheManager, Logger logger)
        {
            _repo = repo;
            _cache = cacheManager.GetCache<Dictionary<Quality, QualityDefinition>>(this.GetType());
            _logger = logger;
        }

        private Dictionary<Quality, QualityDefinition> GetAll()
        {
            return _cache.Get("all", () => _repo.All().Select(WithWeight).ToDictionary(v => v.Quality), TimeSpan.FromSeconds(5.0));
        }

        public void Update(QualityDefinition qualityDefinition)
        {
            _repo.Update(qualityDefinition);

            _cache.Clear();
        }

        public void UpdateMany(List<QualityDefinition> qualityDefinitions)
        {
            _repo.UpdateMany(qualityDefinitions);
        }

        public List<QualityDefinition> All()
        {
            return GetAll().Values.OrderBy(d => d.Weight).ToList();
        }

        public QualityDefinition GetById(int id)
        {
            return GetAll().Values.Single(v => v.Id == id);
        }
        
        public QualityDefinition Get(Quality quality)
        {
            return GetAll()[quality];
        }
        
        private void InsertMissingDefinitions()
        {
            List<QualityDefinition> insertList = new List<QualityDefinition>();
            List<QualityDefinition> updateList = new List<QualityDefinition>();
            
            var allDefinitions = Quality.DefaultQualityDefinitions.OrderBy(d => d.Weight).ToList();
            var existingDefinitions = _repo.All().ToList();

            foreach (var definition in allDefinitions)
            {
                var existing = existingDefinitions.SingleOrDefault(d => d.Quality == definition.Quality);

                if (existing == null)
                {
                    insertList.Add(definition);
                }

                else
                {
                    updateList.Add(existing);
                    existingDefinitions.Remove(existing);
                }
            }

            _repo.InsertMany(insertList);
            _repo.UpdateMany(updateList);
            _repo.DeleteMany(existingDefinitions);
            
            _cache.Clear();
        }

        private static QualityDefinition WithWeight(QualityDefinition definition)
        {
            definition.Weight = Quality.DefaultQualityDefinitions.Single(d => d.Quality == definition.Quality).Weight;

            return definition;
        }

        public void Handle(ApplicationStartedEvent message)
        {
            _logger.Debug("Setting up default quality config");

            InsertMissingDefinitions();
        }
    }
}
