using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.DecisionEngine.ExternalDecisions
{
    public interface IExternalDecisionRepository : IProviderRepository<ExternalDecisionDefinition>
    {
    }

    public class ExternalDecisionRepository : ProviderRepository<ExternalDecisionDefinition>, IExternalDecisionRepository
    {
        public ExternalDecisionRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
