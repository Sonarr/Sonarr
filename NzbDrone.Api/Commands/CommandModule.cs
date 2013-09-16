using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Api.Extensions;
using NzbDrone.Api.Mapping;
using NzbDrone.Api.Validation;
using NzbDrone.Common.Composition;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Messaging;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Commands.Tracking;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ProgressMessaging;


namespace NzbDrone.Api.Commands
{
    public class CommandModule : NzbDroneRestModuleWithSignalR<CommandResource, Command>, IHandle<CommandUpdatedEvent>
    {
        private readonly ICommandExecutor _commandExecutor;
        private readonly IContainer _container;
        private readonly ITrackCommands _trackCommands;

        public CommandModule(ICommandExecutor commandExecutor, IContainer container, ITrackCommands trackCommands)
            : base(commandExecutor)
        {
            _commandExecutor = commandExecutor;
            _container = container;
            _trackCommands = trackCommands;

            GetResourceById = GetCommand;
            CreateResource = StartCommand;
            GetResourceAll = GetAllCommands;

            PostValidator.RuleFor(c => c.Name).NotBlank();
        }

        private CommandResource GetCommand(int id)
        {
            return _trackCommands.GetById(id).InjectTo<CommandResource>();
        }

        private int StartCommand(CommandResource commandResource)
        {
            var commandType =
              _container.GetImplementations(typeof(Command))
                        .Single(c => c.Name.Replace("Command", "")
                        .Equals(commandResource.Name, StringComparison.InvariantCultureIgnoreCase));

            dynamic command = Request.Body.FromJson(commandType);

            var trackedCommand = (Command)_commandExecutor.PublishCommandAsync(command);
            return trackedCommand.Id;
        }

        private List<CommandResource> GetAllCommands()
        {
            return ToListResource(_trackCommands.RunningCommands);
        }

        public void Handle(CommandUpdatedEvent message)
        {
            if (message.Command.SendUpdatesToClient)
            {
                BroadcastResourceChange(ModelAction.Updated, message.Command.Id);
            }
        }
    }
}