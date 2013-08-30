using System;
using System.Linq;
using Nancy;
using NzbDrone.Api.Extensions;
using NzbDrone.Common.Composition;
using NzbDrone.Common.Messaging;
using NzbDrone.Common.Messaging.Tracking;

namespace NzbDrone.Api.Commands
{
    public class CommandModule : NzbDroneRestModule<CommandResource>
    {
        private readonly IMessageAggregator _messageAggregator;
        private readonly IContainer _container;
        private readonly ITrackCommands _trackCommands;

        public CommandModule(IMessageAggregator messageAggregator, IContainer container, ITrackCommands trackCommands)
        {
            _messageAggregator = messageAggregator;
            _container = container;
            _trackCommands = trackCommands;

            Post["/"] = x => RunCommand(ReadResourceFromRequest());
            Get["/"] = x => GetAllCommands();
        }

        private Response RunCommand(CommandResource resource)
        {
            var commandType =
                _container.GetImplementations(typeof(ICommand))
                          .Single(c => c.Name.Replace("Command", "")
                          .Equals(resource.Command, StringComparison.InvariantCultureIgnoreCase));

            dynamic command = Request.Body.FromJson(commandType);
            _messageAggregator.PublishCommand(command);

            return resource.AsResponse(HttpStatusCode.Created);
        }

        private Response GetAllCommands()
        {
            return _trackCommands.AllTracked().AsResponse();
        }
    }
}