using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;
using NzbDrone.SignalR;
using Sonarr.Http;
using Sonarr.Http.REST;
using Sonarr.Http.REST.Attributes;
using BadRequestException = Sonarr.Http.REST.BadRequestException;

namespace Sonarr.Api.V3.EpisodeFiles
{
    [V3ApiController]
    public class EpisodeFileController : RestControllerWithSignalR<EpisodeFileResource, EpisodeFile>,
                                 IHandle<EpisodeFileAddedEvent>,
                                 IHandle<EpisodeFileDeletedEvent>
    {
        private readonly IMediaFileService _mediaFileService;
        private readonly IDeleteMediaFiles _mediaFileDeletionService;
        private readonly ISeriesService _seriesService;
        private readonly ICustomFormatCalculationService _formatCalculator;
        private readonly IUpgradableSpecification _upgradableSpecification;

        public EpisodeFileController(IBroadcastSignalRMessage signalRBroadcaster,
                             IMediaFileService mediaFileService,
                             IDeleteMediaFiles mediaFileDeletionService,
                             ISeriesService seriesService,
                             ICustomFormatCalculationService formatCalculator,
                             IUpgradableSpecification upgradableSpecification)
            : base(signalRBroadcaster)
        {
            _mediaFileService = mediaFileService;
            _mediaFileDeletionService = mediaFileDeletionService;
            _seriesService = seriesService;
            _formatCalculator = formatCalculator;
            _upgradableSpecification = upgradableSpecification;
        }

        protected override EpisodeFileResource GetResourceById(int id)
        {
            var episodeFile = _mediaFileService.Get(id);
            var series = _seriesService.GetSeries(episodeFile.SeriesId);

            var resource = episodeFile.ToResource(series, _upgradableSpecification, _formatCalculator);

            return resource;
        }

        [HttpGet]
        [Produces("application/json")]
        public List<EpisodeFileResource> GetEpisodeFiles(int? seriesId, [FromQuery] List<int> episodeFileIds)
        {
            if (!seriesId.HasValue && !episodeFileIds.Any())
            {
                throw new BadRequestException("seriesId or episodeFileIds must be provided");
            }

            if (seriesId.HasValue)
            {
                var series = _seriesService.GetSeries(seriesId.Value);
                var files = _mediaFileService.GetFilesBySeries(seriesId.Value);

                if (files == null)
                {
                    return new List<EpisodeFileResource>();
                }

                return files.ConvertAll(e => e.ToResource(series, _upgradableSpecification, _formatCalculator))
                            .ToList();
            }
            else
            {
                var episodeFiles = _mediaFileService.Get(episodeFileIds);

                return episodeFiles.GroupBy(e => e.SeriesId)
                                   .SelectMany(f => f.ToList()
                                                     .ConvertAll(e => e.ToResource(_seriesService.GetSeries(f.Key), _upgradableSpecification, _formatCalculator)))
                                   .ToList();
            }
        }

        [RestPutById]
        [Consumes("application/json")]
        public ActionResult<EpisodeFileResource> SetQuality(EpisodeFileResource episodeFileResource)
        {
            var episodeFile = _mediaFileService.Get(episodeFileResource.Id);
            episodeFile.Quality = episodeFileResource.Quality;

            if (episodeFileResource.SceneName != null && SceneChecker.IsSceneTitle(episodeFileResource.SceneName))
            {
                episodeFile.SceneName = episodeFileResource.SceneName;
            }

            if (episodeFileResource.ReleaseGroup != null)
            {
                episodeFile.ReleaseGroup = episodeFileResource.ReleaseGroup;
            }

            _mediaFileService.Update(episodeFile);
            return Accepted(episodeFile.Id);
        }

        [HttpPut("editor")]
        [Consumes("application/json")]
        public object SetQuality([FromBody] EpisodeFileListResource resource)
        {
            var episodeFiles = _mediaFileService.GetFiles(resource.EpisodeFileIds);

            foreach (var episodeFile in episodeFiles)
            {
                if (resource.Languages != null)
                {
                    episodeFile.Languages = resource.Languages;
                }

                if (resource.Quality != null)
                {
                    episodeFile.Quality = resource.Quality;
                }

                if (resource.SceneName != null && SceneChecker.IsSceneTitle(resource.SceneName))
                {
                    episodeFile.SceneName = resource.SceneName;
                }

                if (resource.ReleaseGroup != null)
                {
                    episodeFile.ReleaseGroup = resource.ReleaseGroup;
                }
            }

            _mediaFileService.Update(episodeFiles);

            var series = _seriesService.GetSeries(episodeFiles.First().SeriesId);

            return Accepted(episodeFiles.ConvertAll(f => f.ToResource(series, _upgradableSpecification, _formatCalculator)));
        }

        [RestDeleteById]
        public void DeleteEpisodeFile(int id)
        {
            var episodeFile = _mediaFileService.Get(id);

            if (episodeFile == null)
            {
                throw new NzbDroneClientException(global::System.Net.HttpStatusCode.NotFound, "Episode file not found");
            }

            var series = _seriesService.GetSeries(episodeFile.SeriesId);

            _mediaFileDeletionService.DeleteEpisodeFile(series, episodeFile);
        }

        [HttpDelete("bulk")]
        [Consumes("application/json")]
        public object DeleteEpisodeFiles([FromBody] EpisodeFileListResource resource)
        {
            var episodeFiles = _mediaFileService.GetFiles(resource.EpisodeFileIds);
            var series = _seriesService.GetSeries(episodeFiles.First().SeriesId);

            foreach (var episodeFile in episodeFiles)
            {
                _mediaFileDeletionService.DeleteEpisodeFile(series, episodeFile);
            }

            return new { };
        }

        [HttpPut("bulk")]
        [Consumes("application/json")]
        public object SetPropertiesBulk([FromBody] List<EpisodeFileResource> resources)
        {
            var episodeFiles = _mediaFileService.GetFiles(resources.Select(r => r.Id));

            foreach (var episodeFile in episodeFiles)
            {
                var resourceEpisodeFile = resources.Single(r => r.Id == episodeFile.Id);

                if (resourceEpisodeFile.Languages != null)
                {
                    episodeFile.Languages = resourceEpisodeFile.Languages;
                }

                if (resourceEpisodeFile.Quality != null)
                {
                    episodeFile.Quality = resourceEpisodeFile.Quality;
                }

                if (resourceEpisodeFile.SceneName != null && SceneChecker.IsSceneTitle(resourceEpisodeFile.SceneName))
                {
                    episodeFile.SceneName = resourceEpisodeFile.SceneName;
                }

                if (resourceEpisodeFile.ReleaseGroup != null)
                {
                    episodeFile.ReleaseGroup = resourceEpisodeFile.ReleaseGroup;
                }

                if (resourceEpisodeFile.IndexerFlags.HasValue)
                {
                    episodeFile.IndexerFlags = (IndexerFlags)resourceEpisodeFile.IndexerFlags;
                }

                if (resourceEpisodeFile.ReleaseType != null)
                {
                    episodeFile.ReleaseType = (ReleaseType)resourceEpisodeFile.ReleaseType;
                }
            }

            _mediaFileService.Update(episodeFiles);
            var series = _seriesService.GetSeries(episodeFiles.First().SeriesId);
            return Accepted(episodeFiles.ConvertAll(f => f.ToResource(series, _upgradableSpecification, _formatCalculator)));
        }

        [NonAction]
        public void Handle(EpisodeFileAddedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.EpisodeFile.Id);
        }

        [NonAction]
        public void Handle(EpisodeFileDeletedEvent message)
        {
            BroadcastResourceChange(ModelAction.Deleted, message.EpisodeFile.Id);
        }
    }
}
