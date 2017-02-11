using System.Collections.Generic;
using Sonarr.Http.REST;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Exceptions;
using NzbDrone.SignalR;
using HttpStatusCode = System.Net.HttpStatusCode;

namespace NzbDrone.Api.EpisodeFiles
{
    public class EpisodeFileModule : SonarrRestModuleWithSignalR<EpisodeFileResource, EpisodeFile>,
                                 IHandle<EpisodeFileAddedEvent>
    {
        private readonly IMediaFileService _mediaFileService;
        private readonly IDeleteMediaFiles _mediaFileDeletionService;
        private readonly ISeriesService _seriesService;
        private readonly IQualityUpgradableSpecification _qualityUpgradableSpecification;

        public EpisodeFileModule(IBroadcastSignalRMessage signalRBroadcaster,
                             IMediaFileService mediaFileService,
                             IDeleteMediaFiles mediaFileDeletionService,
                             ISeriesService seriesService,
                             IQualityUpgradableSpecification qualityUpgradableSpecification)
            : base(signalRBroadcaster)
        {
            _mediaFileService = mediaFileService;
            _mediaFileDeletionService = mediaFileDeletionService;
            _seriesService = seriesService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
            GetResourceById = GetEpisodeFile;
            GetResourceAll = GetEpisodeFiles;
            UpdateResource = SetQuality;
            DeleteResource = DeleteEpisodeFile;
        }

        private EpisodeFileResource GetEpisodeFile(int id)
        {
            var episodeFile = _mediaFileService.Get(id);
            var series = _seriesService.GetSeries(episodeFile.SeriesId);

            return episodeFile.ToResource(series, _qualityUpgradableSpecification);
        }

        private List<EpisodeFileResource> GetEpisodeFiles()
        {
            if (!Request.Query.SeriesId.HasValue)
            {
                throw new BadRequestException("seriesId is missing");
            }

            var seriesId = (int)Request.Query.SeriesId;

            var series = _seriesService.GetSeries(seriesId);

            return _mediaFileService.GetFilesBySeries(seriesId).ConvertAll(f => f.ToResource(series, _qualityUpgradableSpecification));
        }

        private void SetQuality(EpisodeFileResource episodeFileResource)
        {
            var episodeFile = _mediaFileService.Get(episodeFileResource.Id);
            episodeFile.Quality = episodeFileResource.Quality;
            _mediaFileService.Update(episodeFile);
        }

        private void DeleteEpisodeFile(int id)
        {
            var episodeFile = _mediaFileService.Get(id);

            if (episodeFile == null)
            {
                throw new NzbDroneClientException(HttpStatusCode.NotFound, "Episode file not found");
            }

            var series = _seriesService.GetSeries(episodeFile.SeriesId);

            _mediaFileDeletionService.DeleteEpisodeFile(series, episodeFile);
        }

        public void Handle(EpisodeFileAddedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.EpisodeFile.Id);
        }
    }
}