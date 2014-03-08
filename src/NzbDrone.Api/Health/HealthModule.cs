using System;
using System.Collections.Generic;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.HealthCheck;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Api.Health
{
    public class HealthModule : NzbDroneRestModuleWithSignalR<HealthResource, HealthCheck>,
                                IHandle<TriggerHealthCheckEvent>
    {
        private readonly IHealthCheckService _healthCheckService;

        public HealthModule(ICommandExecutor commandExecutor, IHealthCheckService healthCheckService)
            : base(commandExecutor)
        {
            _healthCheckService = healthCheckService;
            GetResourceAll = GetHealth;
        }

        private List<HealthResource> GetHealth()
        {
            return ToListResource(_healthCheckService.PerformHealthCheck);
        }

        public void Handle(TriggerHealthCheckEvent message)
        {
            BroadcastResourceChange(ModelAction.Sync);
        }
    }
}