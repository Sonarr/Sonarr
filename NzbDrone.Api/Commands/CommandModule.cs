using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Api.Extensions;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Api.Commands
{
    public class CommandModule : NzbDroneRestModule<CommandResource>
    {
        private readonly IMessageAggregator _messageAggregator;
        private readonly IEnumerable<ICommand> _commands;

        public CommandModule(IMessageAggregator messageAggregator, IEnumerable<ICommand> commands)
        {
            _messageAggregator = messageAggregator;
            _commands = commands;

            CreateResource = RunCommand;
        }

        private CommandResource RunCommand(CommandResource resource)
        {
            var commandType = _commands.Single(c => c.GetType().Name.Replace("Command", "").Equals(resource.Command, StringComparison.InvariantCultureIgnoreCase))
                                       .GetType();
            var command = (object)Request.Body.FromJson<ICommand>(commandType);
            var method = typeof(IMessageAggregator).GetMethod("PublishCommand");
            var genericMethod = method.MakeGenericMethod(commandType);
            genericMethod.Invoke(_messageAggregator, new[] { command });

            return resource;

        }
    }
}