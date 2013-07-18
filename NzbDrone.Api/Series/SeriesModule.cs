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

        private SeriesResource AddSeries(SeriesResource seriesResource)
        {
            return ToResource<Core.Tv.Series>(_seriesService.AddSeries, seriesResource);
        }

        private SeriesResource UpdateSeries(SeriesResource seriesResource)
        {
            var resource = ToResource<Core.Tv.Series>(_seriesService.UpdateSeries, seriesResource);
            MapCoversToLocal(resource);
            FetchAndLinkSeriesStatistics(resource);

            return resource;
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
            resource.SeasonCount = seriesStatistics.SeasonCount;
            resource.NextAiring = seriesStatistics.NextAiring;
        }
    }
}
