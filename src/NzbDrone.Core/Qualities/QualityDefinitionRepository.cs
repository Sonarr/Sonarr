using System;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;


namespace NzbDrone.Core.Qualities
{
    public interface IQualityDefinitionRepository : IBasicRepository<QualityDefinition>
    {
        QualityDefinition GetByQualityId(int qualityId);
    }

    public class QualityDefinitionRepository : BasicRepository<QualityDefinition>, IQualityDefinitionRepository
    {
        public QualityDefinitionRepository(IDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public QualityDefinition GetByQualityId(int qualityId)
        {
            try
            {
                return Query.Where(q => (int) q.Quality == qualityId).Single();
            }
            catch (InvalidOperationException e)
            {
                throw new ModelNotFoundException(typeof(QualityDefinition), qualityId);
            }
        }
    }
}
