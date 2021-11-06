using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.HealthCheck;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.SignalR;
using Sonarr.Http;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.Health
{
    [V3ApiController]
    public class HealthController : RestControllerWithSignalR<HealthResource, HealthCheck>,
                                IHandle<HealthCheckCompleteEvent>
    {
        private readonly IHealthCheckService _healthCheckService;

        public HealthController(IBroadcastSignalRMessage signalRBroadcaster, IHealthCheckService healthCheckService)
            : base(signalRBroadcaster)
        {
            _healthCheckService = healthCheckService;
        }

        protected override HealthResource GetResourceById(int id)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        public List<HealthResource> GetHealth()
        {
            return _healthCheckService.Results().ToResource();
        }

        [NonAction]
        public void Handle(HealthCheckCompleteEvent message)
        {
            BroadcastResourceChange(ModelAction.Sync);
        }
    }
}
