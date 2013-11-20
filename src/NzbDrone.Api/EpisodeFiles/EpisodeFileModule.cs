using System.Collections.Generic;
using NzbDrone.Api.REST;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Api.Mapping;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Api.EpisodeFiles
{
    public class EpisodeModule : NzbDroneRestModuleWithSignalR<EpisodeFileResource, EpisodeFile>,
                                 IHandle<EpisodeFileAddedEvent>
    {
        private readonly IMediaFileService _mediaFileService;

        public EpisodeModule(ICommandExecutor commandExecutor, IMediaFileService mediaFileService)
            : base(commandExecutor)
        {
            _mediaFileService = mediaFileService;
            GetResourceById = GetEpisodeFile;
            GetResourceAll = GetEpisodeFiles;
            UpdateResource = SetQuality;
        }

        private EpisodeFileResource GetEpisodeFile(int id)
        {
            return _mediaFileService.Get(id).InjectTo<EpisodeFileResource>();
        }

        private List<EpisodeFileResource> GetEpisodeFiles()
        {
            var seriesId = (int?)Request.Query.SeriesId;

            if (seriesId == null)
            {
                throw new BadRequestException("seriesId is missing");
            }

            return ToListResource(() => _mediaFileService.GetFilesBySeries(seriesId.Value));
        }

        private void SetQuality(EpisodeFileResource episodeFileResource)
        {
            var episodeFile = _mediaFileService.Get(episodeFileResource.Id);
            episodeFile.Quality = episodeFileResource.Quality;
            _mediaFileService.Update(episodeFile);
        }

        public void Handle(EpisodeFileAddedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.EpisodeFile.Id);
        }
    }
}