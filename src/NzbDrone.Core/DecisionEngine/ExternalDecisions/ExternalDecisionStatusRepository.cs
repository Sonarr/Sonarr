using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider.Status;

namespace NzbDrone.Core.DecisionEngine.ExternalDecisions
{
    public interface IExternalDecisionStatusRepository : IProviderStatusRepository<ExternalDecisionStatus>
    {
    }

    public class ExternalDecisionStatusRepository : ProviderStatusRepository<ExternalDecisionStatus>, IExternalDecisionStatusRepository
    {
        public ExternalDecisionStatusRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
