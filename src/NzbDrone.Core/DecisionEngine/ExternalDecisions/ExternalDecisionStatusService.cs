using System;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider.Status;

namespace NzbDrone.Core.DecisionEngine.ExternalDecisions
{
    public interface IExternalDecisionStatusService : IProviderStatusServiceBase<ExternalDecisionStatus>
    {
    }

    public class ExternalDecisionStatusService : ProviderStatusServiceBase<IExternalDecision, ExternalDecisionStatus>, IExternalDecisionStatusService
    {
        public ExternalDecisionStatusService(IExternalDecisionStatusRepository providerStatusRepository, IEventAggregator eventAggregator, IRuntimeInfo runtimeInfo, Logger logger)
            : base(providerStatusRepository, eventAggregator, runtimeInfo, logger)
        {
            MinimumTimeSinceInitialFailure = TimeSpan.FromMinutes(5);
            MaximumEscalationLevel = 5;
        }
    }
}
