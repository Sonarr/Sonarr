using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.SignalR;
using Sonarr.Http;
using Sonarr.Http.REST;
using Sonarr.Http.REST.Attributes;

namespace Sonarr.Api.V5.Qualities;

[V5ApiController]
public class QualityDefinitionController :
    RestControllerWithSignalR<QualityDefinitionResource, QualityDefinition>,
    IHandle<CommandExecutedEvent>
{
    private readonly IQualityDefinitionService _qualityDefinitionService;
    private readonly IQualityProfileService _qualityProfileService;

    public QualityDefinitionController(
        IQualityDefinitionService qualityDefinitionService,
        IQualityProfileService qualityProfileService,
        IBroadcastSignalRMessage signalRBroadcaster)
        : base(signalRBroadcaster)
    {
        _qualityDefinitionService = qualityDefinitionService;
        _qualityProfileService = qualityProfileService;
    }

    [RestPutById]
    public ActionResult<QualityDefinitionResource> Update([FromBody] QualityDefinitionResource resource)
    {
        var model = resource.ToModel();
        _qualityDefinitionService.Update(model);

        if (model.MinSize.HasValue || model.MaxSize.HasValue || model.PreferredSize.HasValue)
        {
            _qualityProfileService.UpdateAllSizeLimits(new QualityProfileSizeLimit(model));
        }

        return Accepted(model.Id);
    }

    protected override QualityDefinitionResource GetResourceById(int id)
    {
        return _qualityDefinitionService.GetById(id).ToResource();
    }

    [HttpGet]
    public List<QualityDefinitionResource> GetAll()
    {
        return _qualityDefinitionService.All().ToResource();
    }

    [HttpPut]
    public object UpdateMany([FromBody] List<QualityDefinitionResource> resource)
    {
        // Read from request
        var qualityDefinitions = resource.ToModel().ToList();

        _qualityDefinitionService.UpdateMany(qualityDefinitions);

        var toUpdate = qualityDefinitions
            .Where(q => q.MinSize.HasValue || q.MaxSize.HasValue || q.PreferredSize.HasValue)
            .Select(q => new QualityProfileSizeLimit(q))
            .ToArray();

        if (toUpdate.Any())
        {
            _qualityProfileService.UpdateAllSizeLimits(toUpdate);
        }

        return Accepted(_qualityDefinitionService.All()
            .ToResource());
    }

    [NonAction]
    public void Handle(CommandExecutedEvent message)
    {
        if (message.Command.Name == "ResetQualityDefinitions")
        {
            BroadcastResourceChange(ModelAction.Sync);
        }
    }
}
