using NzbDrone.Api.Mapping;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.Episodes
{
    public abstract class EpisodeModuleWithSignalR : NzbDroneRestModuleWithSignalR<EpisodeResource, Episode>,
        IHandle<EpisodeGrabbedEvent>,                         
        IHandle<EpisodeDownloadedEvent>
    {
        private readonly IEpisodeService _episodeService;

        protected EpisodeModuleWithSignalR(IEpisodeService episodeService, ICommandExecutor commandExecutor)
            : base(commandExecutor)
        {
            _episodeService = episodeService;

            GetResourceById = GetEpisode;
        }

        protected EpisodeModuleWithSignalR(IEpisodeService episodeService, ICommandExecutor commandExecutor, string resource)
            : base(commandExecutor, resource)
        {
            _episodeService = episodeService;

            GetResourceById = GetEpisode;
        }

        protected EpisodeResource GetEpisode(int id)
        {
            var episode = _episodeService.GetEpisode(id);
            episode.EpisodeFile.LazyLoad();
            return episode.InjectTo<EpisodeResource>();
        }

        public void Handle(EpisodeGrabbedEvent message)
        {
            foreach (var episode in message.Episode.Episodes)
            {
                var resource = episode.InjectTo<EpisodeResource>();
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