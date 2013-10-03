using NzbDrone.Api.REST;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.SignalR;

namespace NzbDrone.Api
{
    public abstract class NzbDroneRestModuleWithSignalR<TResource, TModel> : NzbDroneRestModule<TResource>, IHandle<ModelEvent<TModel>>
        where TResource : RestResource, new()
        where TModel : ModelBase
    {
        private readonly ICommandExecutor _commandExecutor;

        protected NzbDroneRestModuleWithSignalR(ICommandExecutor commandExecutor)
        {
            _commandExecutor = commandExecutor;
        }

        protected NzbDroneRestModuleWithSignalR(ICommandExecutor commandExecutor, string resource)
            : base(resource)
        {
            _commandExecutor = commandExecutor;
        }

        public void Handle(ModelEvent<TModel> message)
        {
            if (message.Action == ModelAction.Deleted || message.Action == ModelAction.Sync)
            {
                BroadcastResourceChange(message.Action);
            }

            BroadcastResourceChange(message.Action, message.Model.Id);
        }

        protected void BroadcastResourceChange(ModelAction action, TResource resource)
        {
            var signalRMessage = new SignalRMessage
            {
                Name = Resource,
                Body = new ResourceChangeMessage<TResource>(resource, action)
            };

            _commandExecutor.PublishCommand(new BroadcastSignalRMessage(signalRMessage));
        }

        protected void BroadcastResourceChange(ModelAction action, int id)
        {
            var resource = GetResourceById(id);

            var signalRMessage = new SignalRMessage
            {
                Name = Resource,
                Body = new ResourceChangeMessage<TResource>(resource, action)
            };

            _commandExecutor.PublishCommand(new BroadcastSignalRMessage(signalRMessage));
        }

        protected void BroadcastResourceChange(ModelAction action)
        {
            var signalRMessage = new SignalRMessage
            {
                Name = Resource,
                Body = new ResourceChangeMessage<TResource>(action)
            };

            _commandExecutor.PublishCommand(new BroadcastSignalRMessage(signalRMessage));
        }
    }
}