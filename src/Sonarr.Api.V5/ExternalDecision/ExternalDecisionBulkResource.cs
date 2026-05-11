using NzbDrone.Core.DecisionEngine.ExternalDecisions;
using Sonarr.Api.V5.Provider;

namespace Sonarr.Api.V5.ExternalDecision;

public class ExternalDecisionBulkResource : ProviderBulkResource<ExternalDecisionBulkResource>
{
}

public class ExternalDecisionBulkResourceMapper : ProviderBulkResourceMapper<ExternalDecisionBulkResource, ExternalDecisionDefinition>
{
}
