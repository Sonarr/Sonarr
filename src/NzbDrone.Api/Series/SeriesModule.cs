using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.SeriesStats;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;
using NzbDrone.Core.Validation.Paths;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.Validation;
using NzbDrone.SignalR;
using Sonarr.Http;
using Sonarr.Http.Mapping;

namespace NzbDrone.Api.Series
{
    public class SeriesModule : SonarrRestModuleWithSignalR<SeriesResource, Core.Tv.Series>, 
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

        public SeriesModule(IBroadcastSignalRMessage signalRBroadcaster,
                            ISeriesService seriesService,
                            IAddSeriesService addSeriesService,
                            ISeriesStatisticsService seriesStatisticsService,
                            ISceneMappingService sceneMappingService,
                            IMapCoversToLocal coverMapper,
                            RootFolderValidator rootFolderValidator,
                            SeriesPathValidator seriesPathValidator,
                            SeriesExistsValidator seriesExistsValidator,
                            DroneFactoryValidator droneFactoryValidator,
                            SeriesAncestorValidator seriesAncestorValidator,
                            ProfileExistsValidator profileExistsValidator,
                            LanguageProfileExistsValidator languageProfileExistsValidator
            )
            : base(signalRBroadcaster)
        {
            _seriesService = seriesService;
            _addSeriesService = addSeriesService;
            _seriesStatisticsService = seriesStatisticsService;
            _sceneMappingService = sceneMappingService;

            _coverMapper = coverMapper;

            GetResourceAll = AllSeries;
            GetResourceById = GetSeries;
            CreateResource = AddSeries;
            UpdateResource = UpdateSeries;
            DeleteResource = DeleteSeries;

            SharedValidator.RuleFor(s => s.ProfileId).ValidId();
            SharedValidator.RuleFor(s => s.LanguageProfileId);

            SharedValidator.RuleFor(s => s.Path)
                           .Cascade(CascadeMode.StopOnFirstFailure)
                           .IsValidPath()
                           .SetValidator(rootFolderValidator)
                           .SetValidator(seriesPathValidator)
                           .SetValidator(droneFactoryValidator)
                           .SetValidator(seriesAncestorValidator)
                           .When(s => !s.Path.IsNullOrWhiteSpace());

            SharedValidator.RuleFor(s => s.ProfileId).SetValidator(profileExistsValidator);
            SharedValidator.RuleFor(s => s.LanguageProfileId).SetValidator(languageProfileExistsValidator);

            PostValidator.RuleFor(s => s.Path).IsValidPath().When(s => s.RootFolderPath.IsNullOrWhiteSpace());
            PostValidator.RuleFor(s => s.RootFolderPath).IsValidPath().When(s => s.Path.IsNullOrWhiteSpace());
            PostValidator.RuleFor(s => s.TvdbId).GreaterThan(0).SetValidator(seriesExistsValidator);

            PutValidator.RuleFor(s => s.Path).IsValidPath();
        }

        private SeriesResource GetSeries(int id)
        {
            var series = _seriesService.GetSeries(id);
            return MapToResource(series);
        }

        private SeriesResource MapToResource(Core.Tv.Series series)
        {
            if (series == null) return null;

            var resource = series.ToResource();
            MapCoversToLocal(resource);
            FetchAndLinkSeriesStatistics(resource);
            PopulateAlternateTitles(resource);

            return resource;
        }

        private List<SeriesResource> AllSeries()
        {
            var seriesStats = _seriesStatisticsService.SeriesStatistics();
            var seriesResources = _seriesService.GetAllSeries().ToResource();

            MapCoversToLocal(seriesResources.ToArray());
            LinkSeriesStatistics(seriesResources, seriesStats);
            PopulateAlternateTitles(seriesResources);

            return seriesResources;
        }

        private int AddSeries(SeriesResource seriesResource)
        {
            var model = seriesResource.ToModel();

            return _addSeriesService.AddSeries(model).Id;
        }

        private void UpdateSeries(SeriesResource seriesResource)
        {
            var model = seriesResource.ToModel(_seriesService.GetSeries(seriesResource.Id));

            _seriesService.UpdateSeries(model);

            BroadcastResourceChange(ModelAction.Updated, seriesResource);
        }

        private void DeleteSeries(int id)
        {
            var deleteFiles = false;
            var deleteFilesQuery = Request.Query.deleteFiles;

            if (deleteFilesQuery.HasValue)
            {
                deleteFiles = Convert.ToBoolean(deleteFilesQuery.Value);
            }

            _seriesService.DeleteSeries(id, deleteFiles);
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
            var dictSeriesStats = seriesStatistics.ToDictionary(v => v.SeriesId);

            foreach (var series in resources)
            {
                var stats = dictSeriesStats.GetValueOrDefault(series.Id);
                if (stats == null) continue;

                LinkSeriesStatistics(series, stats);
            }
        }

        private void LinkSeriesStatistics(SeriesResource resource, SeriesStatistics seriesStatistics)
        {
            resource.TotalEpisodeCount = seriesStatistics.TotalEpisodeCount;
            resource.EpisodeCount = seriesStatistics.EpisodeCount;
            resource.EpisodeFileCount = seriesStatistics.EpisodeFileCount;
            resource.NextAiring = seriesStatistics.NextAiring;
            resource.PreviousAiring = seriesStatistics.PreviousAiring;
            resource.SizeOnDisk = seriesStatistics.SizeOnDisk;

            if (seriesStatistics.SeasonStatistics != null)
            {
                var dictSeasonStats = seriesStatistics.SeasonStatistics.ToDictionary(v => v.SeasonNumber);

                foreach (var season in resource.Seasons)
                {
                    season.Statistics = dictSeasonStats.GetValueOrDefault(season.SeasonNumber).ToResource();
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

            if (mappings == null) return;

            resource.AlternateTitles = mappings.Select(v => new AlternateTitleResource
                                                            {
                                                                Title = v.Title,
                                                                SeasonNumber = v.SeasonNumber,
                                                                SceneSeasonNumber = v.SceneSeasonNumber
                                                            }).ToList();
        }

        public void Handle(EpisodeImportedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.ImportedEpisode.SeriesId);
        }

        public void Handle(EpisodeFileDeletedEvent message)
        {
            if (message.Reason == DeleteMediaFileReason.Upgrade) return;

            BroadcastResourceChange(ModelAction.Updated, message.EpisodeFile.SeriesId);
        }

        public void Handle(SeriesUpdatedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.Series.Id);
        }

        public void Handle(SeriesEditedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.Series.Id);
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
            BroadcastResourceChange(ModelAction.Updated, message.Series.Id);
        }
    }
}
