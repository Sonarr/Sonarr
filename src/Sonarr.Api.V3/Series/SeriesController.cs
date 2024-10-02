using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.SeriesStats;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Commands;
using NzbDrone.Core.Tv.Events;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;
using NzbDrone.SignalR;
using Sonarr.Http;
using Sonarr.Http.Extensions;
using Sonarr.Http.REST;
using Sonarr.Http.REST.Attributes;

namespace Sonarr.Api.V3.Series
{
    [V3ApiController]
    public class SeriesController : RestControllerWithSignalR<SeriesResource, NzbDrone.Core.Tv.Series>,
                                IHandle<EpisodeImportedEvent>,
                                IHandle<EpisodeFileDeletedEvent>,
                                IHandle<SeriesUpdatedEvent>,
                                IHandle<SeriesEditedEvent>,
                                IHandle<SeriesDeletedEvent>,
                                IHandle<SeriesRenamedEvent>,
                                IHandle<MediaCoversUpdatedEvent>
    {
        private readonly ISeriesService _seriesService;
        private readonly IAddSeriesService _addSeriesService;
        private readonly ISeriesStatisticsService _seriesStatisticsService;
        private readonly ISceneMappingService _sceneMappingService;
        private readonly IMapCoversToLocal _coverMapper;
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly IRootFolderService _rootFolderService;

        public SeriesController(IBroadcastSignalRMessage signalRBroadcaster,
                            ISeriesService seriesService,
                            IAddSeriesService addSeriesService,
                            ISeriesStatisticsService seriesStatisticsService,
                            ISceneMappingService sceneMappingService,
                            IMapCoversToLocal coverMapper,
                            IManageCommandQueue commandQueueManager,
                            IRootFolderService rootFolderService,
                            RootFolderValidator rootFolderValidator,
                            MappedNetworkDriveValidator mappedNetworkDriveValidator,
                            SeriesPathValidator seriesPathValidator,
                            SeriesExistsValidator seriesExistsValidator,
                            SeriesAncestorValidator seriesAncestorValidator,
                            SystemFolderValidator systemFolderValidator,
                            QualityProfileExistsValidator qualityProfileExistsValidator,
                            RootFolderExistsValidator rootFolderExistsValidator,
                            SeriesFolderAsRootFolderValidator seriesFolderAsRootFolderValidator)
            : base(signalRBroadcaster)
        {
            _seriesService = seriesService;
            _addSeriesService = addSeriesService;
            _seriesStatisticsService = seriesStatisticsService;
            _sceneMappingService = sceneMappingService;

            _coverMapper = coverMapper;
            _commandQueueManager = commandQueueManager;
            _rootFolderService = rootFolderService;

            SharedValidator.RuleFor(s => s.Path).Cascade(CascadeMode.Stop)
                .IsValidPath()
                .SetValidator(rootFolderValidator)
                .SetValidator(mappedNetworkDriveValidator)
                .SetValidator(seriesPathValidator)
                .SetValidator(seriesAncestorValidator)
                .SetValidator(systemFolderValidator)
                .When(s => s.Path.IsNotNullOrWhiteSpace());

            PostValidator.RuleFor(s => s.Path).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .IsValidPath()
                .When(s => s.RootFolderPath.IsNullOrWhiteSpace());
            PostValidator.RuleFor(s => s.RootFolderPath).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .IsValidPath()
                .SetValidator(rootFolderExistsValidator)
                .SetValidator(seriesFolderAsRootFolderValidator)
                .When(s => s.Path.IsNullOrWhiteSpace());

            PutValidator.RuleFor(s => s.Path).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .IsValidPath();

            SharedValidator.RuleFor(s => s.QualityProfileId).Cascade(CascadeMode.Stop)
                .ValidId()
                .SetValidator(qualityProfileExistsValidator);

            PostValidator.RuleFor(s => s.Title).NotEmpty();
            PostValidator.RuleFor(s => s.TvdbId).GreaterThan(0).SetValidator(seriesExistsValidator);
        }

        [HttpGet]
        [Produces("application/json")]
        public List<SeriesResource> AllSeries(int? tvdbId, bool includeSeasonImages = false)
        {
            var seriesStats = _seriesStatisticsService.SeriesStatistics();
            var seriesResources = new List<SeriesResource>();

            if (tvdbId.HasValue)
            {
                seriesResources.AddIfNotNull(_seriesService.FindByTvdbId(tvdbId.Value).ToResource(includeSeasonImages));
            }
            else
            {
                seriesResources.AddRange(_seriesService.GetAllSeries().Select(s => s.ToResource(includeSeasonImages)));
            }

            MapCoversToLocal(seriesResources.ToArray());
            LinkSeriesStatistics(seriesResources, seriesStats.ToDictionary(x => x.SeriesId));
            PopulateAlternateTitles(seriesResources);
            seriesResources.ForEach(LinkRootFolderPath);

            return seriesResources;
        }

        [NonAction]
        public override ActionResult<SeriesResource> GetResourceByIdWithErrorHandler(int id)
        {
            return base.GetResourceByIdWithErrorHandler(id);
        }

        [RestGetById]
        [Produces("application/json")]
        public ActionResult<SeriesResource> GetResourceByIdWithErrorHandler(int id, [FromQuery] bool includeSeasonImages = false)
        {
            try
            {
                return GetSeriesResourceById(id, includeSeasonImages);
            }
            catch (ModelNotFoundException)
            {
                return NotFound();
            }
        }

        protected override SeriesResource GetResourceById(int id)
        {
            var includeSeasonImages = Request?.GetBooleanQueryParameter("includeSeasonImages", false) ?? false;

            // Parse IncludeImages and use it
            return GetSeriesResourceById(id, includeSeasonImages);
        }

        private SeriesResource GetSeriesResourceById(int id, bool includeSeasonImages = false)
        {
            var series = _seriesService.GetSeries(id);

            // Parse IncludeImages and use it
            return GetSeriesResource(series, includeSeasonImages);
        }

        [RestPostById]
        [Consumes("application/json")]
        [Produces("application/json")]
        public ActionResult<SeriesResource> AddSeries([FromBody] SeriesResource seriesResource)
        {
            var series = _addSeriesService.AddSeries(seriesResource.ToModel());

            return Created(series.Id);
        }

        [RestPutById]
        [Consumes("application/json")]
        [Produces("application/json")]
        public ActionResult<SeriesResource> UpdateSeries([FromBody] SeriesResource seriesResource, [FromQuery] bool moveFiles = false)
        {
            var series = _seriesService.GetSeries(seriesResource.Id);

            if (moveFiles)
            {
                var sourcePath = series.Path;
                var destinationPath = seriesResource.Path;

                _commandQueueManager.Push(new MoveSeriesCommand
                {
                    SeriesId = series.Id,
                    SourcePath = sourcePath,
                    DestinationPath = destinationPath,
                    Trigger = CommandTrigger.Manual
                });
            }

            var model = seriesResource.ToModel(series);

            _seriesService.UpdateSeries(model);

            BroadcastResourceChange(ModelAction.Updated, seriesResource);

            return Accepted(seriesResource.Id);
        }

        [RestDeleteById]
        public void DeleteSeries(int id, bool deleteFiles = false, bool addImportListExclusion = false)
        {
            _seriesService.DeleteSeries(new List<int> { id }, deleteFiles, addImportListExclusion);
        }

        private SeriesResource GetSeriesResource(NzbDrone.Core.Tv.Series series, bool includeSeasonImages)
        {
            if (series == null)
            {
                return null;
            }

            var resource = series.ToResource(includeSeasonImages);
            MapCoversToLocal(resource);
            FetchAndLinkSeriesStatistics(resource);
            PopulateAlternateTitles(resource);
            LinkRootFolderPath(resource);

            return resource;
        }

        private void MapCoversToLocal(params SeriesResource[] series)
        {
            foreach (var seriesResource in series)
            {
                _coverMapper.ConvertToLocalUrls(seriesResource.Id, seriesResource.Images);
            }
        }

        private void FetchAndLinkSeriesStatistics(SeriesResource resource)
        {
            LinkSeriesStatistics(resource, _seriesStatisticsService.SeriesStatistics(resource.Id));
        }

        private void LinkSeriesStatistics(List<SeriesResource> resources, Dictionary<int, SeriesStatistics> seriesStatistics)
        {
            foreach (var series in resources)
            {
                if (seriesStatistics.TryGetValue(series.Id, out var stats))
                {
                    LinkSeriesStatistics(series, stats);
                }
            }
        }

        private void LinkSeriesStatistics(SeriesResource resource, SeriesStatistics seriesStatistics)
        {
            // Only set last aired from statistics if it's missing from the series itself
            resource.LastAired ??= seriesStatistics.LastAired;

            resource.PreviousAiring = seriesStatistics.PreviousAiring;
            resource.NextAiring = seriesStatistics.NextAiring;
            resource.Statistics = seriesStatistics.ToResource(resource.Seasons);

            if (seriesStatistics.SeasonStatistics != null)
            {
                foreach (var season in resource.Seasons)
                {
                    season.Statistics = seriesStatistics.SeasonStatistics.SingleOrDefault(s => s.SeasonNumber == season.SeasonNumber).ToResource();
                }
            }
        }

        private void PopulateAlternateTitles(List<SeriesResource> resources)
        {
            foreach (var resource in resources)
            {
                PopulateAlternateTitles(resource);
            }
        }

        private void PopulateAlternateTitles(SeriesResource resource)
        {
            var mappings = _sceneMappingService.FindByTvdbId(resource.TvdbId);

            if (mappings == null)
            {
                return;
            }

            resource.AlternateTitles = mappings.ConvertAll(AlternateTitleResourceMapper.ToResource);
        }

        private void LinkRootFolderPath(SeriesResource resource)
        {
            resource.RootFolderPath = _rootFolderService.GetBestRootFolderPath(resource.Path);
        }

        [NonAction]
        public void Handle(EpisodeImportedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.ImportedEpisode.SeriesId);
        }

        [NonAction]
        public void Handle(EpisodeFileDeletedEvent message)
        {
            if (message.Reason == DeleteMediaFileReason.Upgrade)
            {
                return;
            }

            BroadcastResourceChange(ModelAction.Updated, message.EpisodeFile.SeriesId);
        }

        [NonAction]
        public void Handle(SeriesUpdatedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.Series.Id);
        }

        [NonAction]
        public void Handle(SeriesEditedEvent message)
        {
            var resource = GetSeriesResource(message.Series, false);
            resource.EpisodesChanged = message.EpisodesChanged;
            BroadcastResourceChange(ModelAction.Updated, resource);
        }

        [NonAction]
        public void Handle(SeriesDeletedEvent message)
        {
            foreach (var series in message.Series)
            {
                BroadcastResourceChange(ModelAction.Deleted, series.ToResource());
            }
        }

        [NonAction]
        public void Handle(SeriesRenamedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.Series.Id);
        }

        [NonAction]
        public void Handle(MediaCoversUpdatedEvent message)
        {
            if (message.Updated)
            {
                BroadcastResourceChange(ModelAction.Updated, message.Series.Id);
            }
        }
    }
}
