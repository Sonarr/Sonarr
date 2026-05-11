using NzbDrone.Core.DecisionEngine.ExternalDecisions;
using Sonarr.Api.V5.Provider;

namespace Sonarr.Api.V5.ExternalDecision;

public class ExternalDecisionResource : ProviderResource<ExternalDecisionResource>
{
    public bool Enable { get; set; }
    public ExternalDecisionType DecisionType { get; set; }
    public int Priority { get; set; }
}

public class ExternalDecisionResourceMapper : ProviderResourceMapper<ExternalDecisionResource, ExternalDecisionDefinition>
{
    public override ExternalDecisionResource ToResource(ExternalDecisionDefinition definition)
    {
        var resource = base.ToResource(definition);

        resource.Enable = definition.Enable;
        resource.DecisionType = definition.DecisionType;
        resource.Priority = definition.Priority;

        return resource;
    }

    public override ExternalDecisionDefinition ToModel(ExternalDecisionResource resource, ExternalDecisionDefinition? existingDefinition)
    {
        var definition = base.ToModel(resource, existingDefinition);

        definition.Enable = resource.Enable;
        definition.DecisionType = resource.DecisionType;
        definition.Priority = resource.Priority;

        return definition;
    }
}
