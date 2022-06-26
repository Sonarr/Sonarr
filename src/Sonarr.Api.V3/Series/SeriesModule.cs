using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DataAugmentation.Scene;
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

namespace Sonarr.Api.V3.Series
{
    public class SeriesModule : SonarrRestModuleWithSignalR<SeriesResource, NzbDrone.Core.Tv.Series>,
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

        public SeriesModule(IBroadcastSignalRMessage signalRBroadcaster,
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
                            ProfileExistsValidator profileExistsValidator,
                            LanguageProfileExistsValidator languageProfileExistsValidator,
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

            GetResourceAll = AllSeries;
            GetResourceById = GetSeries;
            CreateResource = AddSeries;
            UpdateResource = UpdateSeries;
            DeleteResource = DeleteSeries;

            Http.Validation.RuleBuilderExtensions.ValidId(SharedValidator.RuleFor(s => s.QualityProfileId));

            SharedValidator.RuleFor(s => s.Path)
                           .Cascade(CascadeMode.StopOnFirstFailure)
                           .IsValidPath()
                           .SetValidator(rootFolderValidator)
                           .SetValidator(mappedNetworkDriveValidator)
                           .SetValidator(seriesPathValidator)
                           .SetValidator(seriesAncestorValidator)
                           .SetValidator(systemFolderValidator)
                           .When(s => !s.Path.IsNullOrWhiteSpace());

            SharedValidator.RuleFor(s => s.QualityProfileId).SetValidator(profileExistsValidator);
            SharedValidator.RuleFor(s => s.LanguageProfileId).SetValidator(languageProfileExistsValidator);

            PostValidator.RuleFor(s => s.Path).IsValidPath().When(s => s.RootFolderPath.IsNullOrWhiteSpace());
            PostValidator.RuleFor(s => s.RootFolderPath)
                         .IsValidPath()
                         .SetValidator(seriesFolderAsRootFolderValidator)
                         .When(s => s.Path.IsNullOrWhiteSpace());
            PostValidator.RuleFor(s => s.Title).NotEmpty();
            PostValidator.RuleFor(s => s.TvdbId).GreaterThan(0).SetValidator(seriesExistsValidator);

            PutValidator.RuleFor(s => s.Path).IsValidPath();
        }

        private List<SeriesResource> AllSeries()
        {
            var tvdbId = Request.GetIntegerQueryParameter("tvdbId");
            var includeSeasonImages = Request.GetBooleanQueryParameter("includeSeasonImages");
            var seriesStats = _seriesStatisticsService.SeriesStatistics();
            var seriesResources = new List<SeriesResource>();

            if (tvdbId > 0)
            {
                seriesResources.AddIfNotNull(_seriesService.FindByTvdbId(tvdbId).ToResource(includeSeasonImages));
            }
            else
            {
                seriesResources.AddRange(_seriesService.GetAllSeries().Select(s => s.ToResource(includeSeasonImages)));
            }

            MapCoversToLocal(seriesResources.ToArray());
            LinkSeriesStatistics(seriesResources, seriesStats);
            PopulateAlternateTitles(seriesResources);
            seriesResources.ForEach(LinkRootFolderPath);

            return seriesResources;
        }

        private SeriesResource GetSeries(int id)
        {
            var includeSeasonImages = Context != null && Request.GetBooleanQueryParameter("includeSeasonImages");
            var series = _seriesService.GetSeries(id);

            return GetSeriesResource(series, includeSeasonImages);
        }

        private int AddSeries(SeriesResource seriesResource)
        {
            var series = _addSeriesService.AddSeries(seriesResource.ToModel());

            return series.Id;
        }

        private void UpdateSeries(SeriesResource seriesResource)
        {
            var moveFiles = Request.GetBooleanQueryParameter("moveFiles");
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
        }

        private void DeleteSeries(int id)
        {
            var deleteFiles = Request.GetBooleanQueryParameter("deleteFiles");
            var addImportListExclusion = Request.GetBooleanQueryParameter("addImportListExclusion");

            _seriesService.DeleteSeries(id, deleteFiles, addImportListExclusion);
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

        private void LinkSeriesStatistics(List<SeriesResource> resources, List<SeriesStatistics> seriesStatistics)
        {
            foreach (var series in resources)
            {
                var stats = seriesStatistics.SingleOrDefault(ss => ss.SeriesId == series.Id);
                if (stats == null)
                {
                    continue;
                }

                LinkSeriesStatistics(series, stats);
            }
        }

        private void LinkSeriesStatistics(SeriesResource resource, SeriesStatistics seriesStatistics)
        {
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

        public void Handle(EpisodeImportedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.ImportedEpisode.SeriesId);
        }

        public void Handle(EpisodeFileDeletedEvent message)
        {
            if (message.Reason == DeleteMediaFileReason.Upgrade)
            {
                return;
            }

            BroadcastResourceChange(ModelAction.Updated, message.EpisodeFile.SeriesId);
        }

        public void Handle(SeriesUpdatedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.Series.Id);
        }

        public void Handle(SeriesEditedEvent message)
        {
            var resource = GetResourceByIdForBroadcast(message.Series.Id);
            resource.EpisodesChanged = message.EpisodesChanged;
            BroadcastResourceChange(ModelAction.Updated, resource);
        }

        public void Handle(SeriesDeletedEvent message)
        {
            BroadcastResourceChange(ModelAction.Deleted, message.Series.ToResource());
        }

        public void Handle(SeriesRenamedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.Series.Id);
        }

        public void Handle(MediaCoversUpdatedEvent message)
        {
            if (message.Updated)
            {
                BroadcastResourceChange(ModelAction.Updated, message.Series.Id);
            }
        }
    }
}
