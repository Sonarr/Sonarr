using NzbDrone.Core.Datastore;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.SignalR;
using Sonarr.Http.REST;

namespace Sonarr.Http
{
    public abstract class SonarrRestModuleWithSignalR<TResource, TModel> : SonarrRestModule<TResource>, IHandle<ModelEvent<TModel>>
        where TResource : RestResource, new()
        where TModel : ModelBase, new()
    {
        private readonly IBroadcastSignalRMessage _signalRBroadcaster;

        protected SonarrRestModuleWithSignalR(IBroadcastSignalRMessage signalRBroadcaster)
        {
            _signalRBroadcaster = signalRBroadcaster;
        }

        protected SonarrRestModuleWithSignalR(IBroadcastSignalRMessage signalRBroadcaster, string resource)
            : base(resource)
        {
            _signalRBroadcaster = signalRBroadcaster;
        }

        public void Handle(ModelEvent<TModel> message)
        {
            if (message.Action == ModelAction.Deleted || message.Action == ModelAction.Sync)
            {
                BroadcastResourceChange(message.Action);
            }

            BroadcastResourceChange(message.Action, message.Model.Id);
        }

        protected void BroadcastResourceChange(ModelAction action, int id)
        {
            if (action == ModelAction.Deleted)
            {
                BroadcastResourceChange(action, new TResource {Id = id});
            }
            else
            {
                var resource = GetResourceById(id);
                BroadcastResourceChange(action, resource);
            }
        }


        protected void BroadcastResourceChange(ModelAction action, TResource resource)
        {
            var signalRMessage = new SignalRMessage
            {
                Name = Resource,
                Body = new ResourceChangeMessage<TResource>(resource, action),
                Action = action
            };

            _signalRBroadcaster.BroadcastMessage(signalRMessage);
        }

     
        protected void BroadcastResourceChange(ModelAction action)
        {
            if (GetType().Namespace.Contains("V3"))
            {
                var signalRMessage = new SignalRMessage
                {
                    Name = Resource,
                    Body = new ResourceChangeMessage<TResource>(action),
                    Action = action
                };

                _signalRBroadcaster.BroadcastMessage(signalRMessage);
            }
        }
    }
}
