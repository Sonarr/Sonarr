using NzbDrone.Api.Mapping;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.Episodes
{
    public abstract class EpisodeModuleWithSignalR<TResource, TModel> : NzbDroneRestModuleWithSignalR<TResource, TModel>,
        IHandle<EpisodeGrabbedEvent>,                         
        IHandle<EpisodeDownloadedEvent>
        where TResource : EpisodeResource, new()
        where TModel : Episode
    {
        protected EpisodeModuleWithSignalR(ICommandExecutor commandExecutor)
            : base(commandExecutor)
        {
        }

        protected EpisodeModuleWithSignalR(ICommandExecutor commandExecutor, string resource)
            : base(commandExecutor, resource)
        {
        }

        public void Handle(EpisodeGrabbedEvent message)
        {
            foreach (var episode in message.Episode.Episodes)
            {
                var resource = episode.InjectTo<TResource>();
                resource.Grabbed = true;

                BroadcastResourceChange(ModelAction.Updated, resource);
            }
        }

        public void Handle(EpisodeDownloadedEvent message)
        {
            foreach (var episode in message.Episode.Episodes)
            {
                BroadcastResourceChange(ModelAction.Updated, episode.Id);
            }
        }
    }
}