using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Qualities;
using NzbDrone.SignalR;
using Sonarr.Http;
using Sonarr.Http.Extensions;

namespace Sonarr.Api.V3.Qualities
{
    public class QualityDefinitionModule : SonarrRestModuleWithSignalR<QualityDefinitionResource, QualityDefinition>, IHandle<CommandExecutedEvent>
    {
        private readonly IQualityDefinitionService _qualityDefinitionService;

        public QualityDefinitionModule(IQualityDefinitionService qualityDefinitionService, IBroadcastSignalRMessage signalRBroadcaster)
            : base(signalRBroadcaster)
        {
            _qualityDefinitionService = qualityDefinitionService;

            GetResourceAll = GetAll;
            GetResourceById = GetById;
            UpdateResource = Update;
            Put("/update",  d => UpdateMany());
        }

        private void Update(QualityDefinitionResource resource)
        {
            var model = resource.ToModel();
            _qualityDefinitionService.Update(model);
        }

        private QualityDefinitionResource GetById(int id)
        {
            return _qualityDefinitionService.GetById(id).ToResource();
        }

        private List<QualityDefinitionResource> GetAll()
        {
            return _qualityDefinitionService.All().ToResource();
        }

        private object UpdateMany()
        {
            //Read from request
            var qualityDefinitions = Request.Body.FromJson<List<QualityDefinitionResource>>()
                                                 .ToModel()
                                                 .ToList();

            _qualityDefinitionService.UpdateMany(qualityDefinitions);

            return ResponseWithCode(_qualityDefinitionService.All()
                                            .ToResource(),
                                            HttpStatusCode.Accepted);
        }

        public void Handle(CommandExecutedEvent message)
        {
            if (message.Command.Name == "ResetQualityDefinitions")
            {
                BroadcastResourceChange(ModelAction.Sync);
            }
        }
    }
}
