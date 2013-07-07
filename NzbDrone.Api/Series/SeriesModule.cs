using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Nancy;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.SeriesStats;
using NzbDrone.Core.Tv;
using NzbDrone.Api.Validation;
using NzbDrone.Api.Extensions;

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

            Get["/{slug}"] = o => GetSeries((string)o.slug.ToString());

            SharedValidator.RuleFor(s => s.QualityProfileId).ValidId();
            SharedValidator.RuleFor(s => s.Path).NotEmpty().When(s => String.IsNullOrEmpty(s.RootFolderPath));
            SharedValidator.RuleFor(s => s.RootFolderPath).NotEmpty().When(s => String.IsNullOrEmpty(s.Path));

            PostValidator.RuleFor(s => s.Title).NotEmpty();
        }

        private Response GetSeries(string slug)
        {
            var series = _seriesService.FindBySlug(slug);

            if (series == null)
            {
                return new NotFoundResponse();
            }


            var resource = ToResource(()=>_seriesService.FindBySlug(slug));

            MapCoversToLocal(resource);

            return resource.AsResponse();
        }

        private List<SeriesResource> AllSeries()
        {
            var seriesStats = _seriesStatisticsService.SeriesStatistics();
            var seriesResources = ToListResource(_seriesService.GetAllSeries);

            foreach (var s in seriesResources)
            {
                var stats = seriesStats.SingleOrDefault(ss => ss.SeriesId == s.Id);
                if (stats == null) continue;

                s.EpisodeCount = stats.EpisodeCount;
                s.EpisodeFileCount = stats.EpisodeFileCount;
                s.SeasonCount = stats.SeasonCount;
                s.NextAiring = stats.NextAiring;
            }

            MapCoversToLocal(seriesResources.ToArray());

            return seriesResources;
        }

        private SeriesResource GetSeries(int id)
        {
            var resource = ToResource(_seriesService.GetSeries, id);

            MapCoversToLocal(resource);

            return resource;
        }

        private SeriesResource AddSeries(SeriesResource seriesResource)
        {
            //Todo: Alert the user if this series already exists
            //Todo: We need to create the folder if the user is adding a new series
            //(we can just create the folder and it won't blow up if it already exists)
            //We also need to remove any special characters from the filename before attempting to create it           

            return ToResource<Core.Tv.Series>(_seriesService.AddSeries, seriesResource);
        }

        private SeriesResource UpdateSeries(SeriesResource seriesResource)
        {
            return ToResource<Core.Tv.Series>(_seriesService.UpdateSeries, seriesResource);
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
    }

}
