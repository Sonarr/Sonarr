using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Nancy;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.SeriesStats;
using NzbDrone.Core.Tv;
using NzbDrone.Api.Validation;
using NzbDrone.Api.Extensions;
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

            Get["/{slug}"] = o => GetSeriesBySlug((string)o.slug.ToString());

            SharedValidator.RuleFor(s => s.QualityProfileId).ValidId();
            SharedValidator.RuleFor(s => s.Path).NotEmpty().When(s => String.IsNullOrEmpty(s.RootFolderPath));
            SharedValidator.RuleFor(s => s.RootFolderPath).NotEmpty().When(s => String.IsNullOrEmpty(s.Path));

            PostValidator.RuleFor(s => s.Title).NotEmpty();
        }

        private SeriesResource GetSeries(int id)
        {
            Core.Tv.Series series = null;

            try
            {
                series = _seriesService.GetSeries(id);
            }
            catch (ModelNotFoundException)
            {
                series = _seriesService.FindBySlug(id.ToString());
            }

            return GetSeriesResource(series);
        }

        private Response GetSeriesBySlug(string slug)
        {
            var series = _seriesService.FindBySlug(slug);

            if (series == null)
            {
                return new NotFoundResponse();
            }

            return GetSeriesResource(series).AsResponse();
        }

        private SeriesResource GetSeriesResource(Core.Tv.Series series)
        {
            if (series == null) return null;

            var resource = series.InjectTo<SeriesResource>();
            MapCoversToLocal(resource);
            LinkSeriesStatistics(resource, _seriesStatisticsService.SeriesStatistics());

            return resource;
        }

        private List<SeriesResource> AllSeries()
        {
            var seriesStats = _seriesStatisticsService.SeriesStatistics();
            var seriesResources = ToListResource(_seriesService.GetAllSeries);

            foreach (var resource in seriesResources)
            {
                LinkSeriesStatistics(resource, seriesStats);
            }

            MapCoversToLocal(seriesResources.ToArray());

            return seriesResources;
        }

        private SeriesResource AddSeries(SeriesResource seriesResource)
        {
            return ToResource<Core.Tv.Series>(_seriesService.AddSeries, seriesResource);
        }

        private SeriesResource UpdateSeries(SeriesResource seriesResource)
        {
            var resource = ToResource<Core.Tv.Series>(_seriesService.UpdateSeries, seriesResource);
            MapCoversToLocal(resource);
            LinkSeriesStatistics(resource, _seriesStatisticsService.SeriesStatistics());

            return resource;
        }

        private void DeleteSeries(int id)
        {
            var deleteFiles = Convert.ToBoolean(Request.Headers["deleteFiles"].FirstOrDefault());
            _seriesService.DeleteSeries(id, deleteFiles);
        }

        private void MapCoversToLocal(params SeriesResource[] series)
        {
            foreach (var seriesResource in series)
            {
                _coverMapper.ConvertToLocalUrls(seriesResource.Id, seriesResource.Images);
            }
        }

        private void LinkSeriesStatistics(SeriesResource resource, List<SeriesStatistics> seriesStatistics)
        {
            var stats = seriesStatistics.SingleOrDefault(ss => ss.SeriesId == resource.Id);
                if (stats == null) return;

            resource.EpisodeCount = stats.EpisodeCount;
            resource.EpisodeFileCount = stats.EpisodeFileCount;
            resource.SeasonCount = stats.SeasonCount;
            resource.NextAiring = stats.NextAiring;
        }
    }
}
