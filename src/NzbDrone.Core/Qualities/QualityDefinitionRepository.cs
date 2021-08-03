using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Qualities
{
    public interface IQualityDefinitionRepository : IBasicRepository<QualityDefinition>
    {
    }

    public class QualityDefinitionRepository : BasicRepository<QualityDefinition>, IQualityDefinitionRepository
    {
        public QualityDefinitionRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
