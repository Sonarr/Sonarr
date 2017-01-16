using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Api.Extensions;
using NzbDrone.Api.Validation;
using NzbDrone.Common;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ProgressMessaging;
using NzbDrone.SignalR;


namespace NzbDrone.Api.Commands
{
    public class CommandModule : NzbDroneRestModuleWithSignalR<CommandResource, CommandModel>, IHandle<CommandUpdatedEvent>
    {
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly IServiceFactory _serviceFactory;

        public CommandModule(IManageCommandQueue commandQueueManager,
                             IBroadcastSignalRMessage signalRBroadcaster,
                             IServiceFactory serviceFactory)
            : base(signalRBroadcaster)
        {
            _commandQueueManager = commandQueueManager;
            _serviceFactory = serviceFactory;

            GetResourceById = GetCommand;
            CreateResource = StartCommand;
            GetResourceAll = GetStartedCommands;

            PostValidator.RuleFor(c => c.Name).NotBlank();
        }

        private CommandResource GetCommand(int id)
        {
            return _commandQueueManager.Get(id).ToResource();
        }

        private int StartCommand(CommandResource commandResource)
        {
            var commandType = _serviceFactory.GetImplementations(typeof(Command))
                                             .Single(c => c.Name.Replace("Command", "").Equals(commandResource.Name, StringComparison.InvariantCultureIgnoreCase));

            dynamic command = Request.Body.FromJson(commandType);
            command.Trigger = CommandTrigger.Manual;

            var trackedCommand = _commandQueueManager.Push(command, CommandPriority.Normal, CommandTrigger.Manual);
            return trackedCommand.Id;
        }

        private List<CommandResource> GetStartedCommands()
        {
            return _commandQueueManager.GetStarted().ToResource();
        }

        public void Handle(CommandUpdatedEvent message)
        {
            if (message.Command.Body.SendUpdatesToClient)
            {
                BroadcastResourceChange(ModelAction.Updated, message.Command.ToResource());
            }
        }
    }
}