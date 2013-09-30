using System;
using System.Collections.Generic;
using NzbDrone.Api.Mapping;
using NzbDrone.Api.REST;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.Episodes
{
    public class EpisodeModule : NzbDroneRestModuleWithSignalR<EpisodeResource, Episode>,
                                 IHandle<EpisodeGrabbedEvent>,                         
                                 IHandle<EpisodeDownloadedEvent>
                                 
    {
        private readonly IEpisodeService _episodeService;

        public EpisodeModule(ICommandExecutor commandExecutor, IEpisodeService episodeService)
            : base(commandExecutor, "episodes")
        {
            _episodeService = episodeService;

            GetResourceAll = GetEpisodes;
            UpdateResource = SetMonitored;
            GetResourceById = GetEpisode;
        }

        private List<EpisodeResource> GetEpisodes()
        {
            var seriesId = (int?)Request.Query.SeriesId;

            if (seriesId == null)
            {
                throw new BadRequestException("seriesId is missing");
            }

            return ToListResource(() => _episodeService.GetEpisodeBySeries(seriesId.Value));
        }

        private void SetMonitored(EpisodeResource episodeResource)
        {
            _episodeService.SetEpisodeMonitored(episodeResource.Id, episodeResource.Monitored);
        }

        private EpisodeResource GetEpisode(int id)
        {
            return _episodeService.GetEpisode(id).InjectTo<EpisodeResource>();
        }

        public void Handle(EpisodeGrabbedEvent message)
        {
            foreach (var episode in message.Episode.Episodes)
            {
                BroadcastResourceChange(ModelAction.Updated, episode.Id);
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