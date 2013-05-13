using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Api.Extensions;
using NzbDrone.Common.Composition;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Api.Commands
{
    public class CommandModule : NzbDroneRestModule<CommandResource>
    {
        private readonly IMessageAggregator _messageAggregator;
        private readonly IContainer _container;

        public CommandModule(IMessageAggregator messageAggregator, IContainer container)
        {
            _messageAggregator = messageAggregator;
            _container = container;

            CreateResource = RunCommand;
        }

        private CommandResource RunCommand(CommandResource resource)
        {
            var commandType =
                _container.GetImplementations(typeof(ICommand))
                          .Single(c => c.Name.Replace("Command", "")
                               .Equals(resource.Command, StringComparison.InvariantCultureIgnoreCase));


            var command = Request.Body.FromJson<ICommand>(commandType);

            _messageAggregator.PublishCommand(command);

            return resource;
        }
    }
}