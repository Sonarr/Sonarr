using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.SeriesStats;
using NzbDrone.Core.Tv;
using NzbDrone.Api.Validation;
using NzbDrone.Api.Mapping;

namespace NzbDrone.Api.Series
{
    public class SeriesModule : NzbDroneRestModule<SeriesResource>
    {
        private readonly ISeriesService _seriesService;
        private readonly ISeriesStatisticsService _seriesStatisticsService;
        private readonly IMapCoversToLocal _coverMapper;

        public SeriesModule(ISeriesService seriesService, ISeriesStatisticsService seriesStatisticsService, IMapCoversToLocal coverMapper)
            : base("/Series")
        {
            _seriesService = seriesService;
            _seriesStatisticsService = seriesStatisticsService;
            _coverMapper = coverMapper;

            GetResourceAll = AllSeries;
            GetResourceById = GetSeries;
            CreateResource = AddSeries;
            UpdateResource = UpdateSeries;
            DeleteResource = DeleteSeries;

            SharedValidator.RuleFor(s => s.QualityProfileId).ValidId();

            PutValidator.RuleFor(s => s.Path).IsValidPath();

            PostValidator.RuleFor(s => s.Path).IsValidPath().When(s => String.IsNullOrEmpty(s.RootFolderPath));
            PostValidator.RuleFor(s => s.RootFolderPath).IsValidPath().When(s => String.IsNullOrEmpty(s.Path));
            PostValidator.RuleFor(s => s.Title).NotEmpty();
        }

        private SeriesResource GetSeries(int id)
        {
            var series = _seriesService.GetSeries(id);
            return GetSeriesResource(series);
        }

        private SeriesResource GetSeriesResource(Core.Tv.Series series)
        {
            if (series == null) return null;

            var resource = series.InjectTo<SeriesResource>();
            MapCoversToLocal(resource);
            FetchAndLinkSeriesStatistics(resource);

            return resource;
        }

        private List<SeriesResource> AllSeries()
        {
            var seriesStats = _seriesStatisticsService.SeriesStatistics();
            var seriesResources = ToListResource(_seriesService.GetAllSeries);

            MapCoversToLocal(seriesResources.ToArray());
            LinkSeriesStatistics(seriesResources, seriesStats);

            return seriesResources;
        }

        private int AddSeries(SeriesResource seriesResource)
        {
            return GetNewId<Core.Tv.Series>(_seriesService.AddSeries, seriesResource);
        }

        private void UpdateSeries(SeriesResource seriesResource)
        {
            GetNewId<Core.Tv.Series>(_seriesService.UpdateSeries, seriesResource);
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
            foreach (var series in resources)
            {
                var stats = seriesStatistics.SingleOrDefault(ss => ss.SeriesId == series.Id);
                if (stats == null) continue;

                LinkSeriesStatistics(series, stats);
            }
        }

        private void LinkSeriesStatistics(SeriesResource resource, SeriesStatistics seriesStatistics)
        {
            resource.EpisodeCount = seriesStatistics.EpisodeCount;
            resource.EpisodeFileCount = seriesStatistics.EpisodeFileCount;
            resource.NextAiring = seriesStatistics.NextAiring;
        }
    }
}
