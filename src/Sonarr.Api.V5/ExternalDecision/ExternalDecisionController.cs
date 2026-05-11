using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.DecisionEngine.ExternalDecisions;
using NzbDrone.SignalR;
using Sonarr.Api.V5.Provider;
using Sonarr.Http;

namespace Sonarr.Api.V5.ExternalDecision;

[V5ApiController]
public class ExternalDecisionController : ProviderControllerBase<ExternalDecisionResource, ExternalDecisionBulkResource, IExternalDecision, ExternalDecisionDefinition>
{
    public static readonly ExternalDecisionResourceMapper ResourceMapper = new();
    public static readonly ExternalDecisionBulkResourceMapper BulkResourceMapper = new();

    public ExternalDecisionController(IBroadcastSignalRMessage signalRBroadcaster, ExternalDecisionFactory externalDecisionFactory)
        : base(signalRBroadcaster, externalDecisionFactory, "externaldecision", ResourceMapper, BulkResourceMapper)
    {
        SharedValidator.RuleFor(c => c.Priority).InclusiveBetween(1, 50);
    }

    [NonAction]
    public override ActionResult<ExternalDecisionResource> UpdateProvider([FromBody] ExternalDecisionBulkResource providerResource)
    {
        throw new NotImplementedException();
    }

    [NonAction]
    public override ActionResult DeleteProviders([FromBody] ExternalDecisionBulkResource resource)
    {
        throw new NotImplementedException();
    }
}
