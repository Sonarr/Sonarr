using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using NzbDrone.Api.Extensions;
using NzbDrone.Common;
using NzbDrone.Common;
using NzbDrone.Core.SeriesStats;
using NzbDrone.Api.Mapping;
using NzbDrone.Core.Tv;
using NzbDrone.Api.Validation;

namespace NzbDrone.Api.Series
{
    public class SeriesModule : NzbDroneRestModule<SeriesResource>
    {
        private readonly ISeriesService _seriesService;
        private readonly ISeriesStatisticsService _seriesStatisticsService;

        public SeriesModule(ISeriesService seriesService, ISeriesStatisticsService seriesStatisticsService)
            : base("/Series")
        {
            _seriesService = seriesService;
            _seriesStatisticsService = seriesStatisticsService;

            GetResourceAll = AllSeries;
            GetResourceById = GetSeries;
            CreateResource = AddSeries;
            UpdateResource = UpdateSeries;
            DeleteResource = DeleteSeries;


            SharedValidator.RuleFor(s => s.RootFolderId).ValidId();
            SharedValidator.RuleFor(s => s.QualityProfileId).ValidId();


            PostValidator.RuleFor(s => s.Title).NotEmpty();

        }

        private List<SeriesResource> AllSeries()
        {
            var series = _seriesService.GetAllSeries().ToList();
            var seriesStats = _seriesStatisticsService.SeriesStatistics();

            var seriesModels = series.InjectTo<List<SeriesResource>>();

            foreach (var s in seriesModels)
            {
                var stats = seriesStats.SingleOrDefault(ss => ss.SeriesId == s.Id);
                if (stats == null) continue;

                s.EpisodeCount = stats.EpisodeCount;
                s.EpisodeFileCount = stats.EpisodeFileCount;
                s.SeasonsCount = stats.NumberOfSeasons;
                s.NextAiring = stats.NextAiring;
            }

            return seriesModels;
        }

        private SeriesResource GetSeries(int id)
        {
            var series = _seriesService.GetSeries(id);
            return series.InjectTo<SeriesResource>();
        }

        private SeriesResource AddSeries(SeriesResource seriesResource)
        {
            var newSeries = Request.Body.FromJson<Core.Tv.Series>();

            //Todo: Alert the user if this series already exists
            //Todo: We need to create the folder if the user is adding a new series
            //(we can just create the folder and it won't blow up if it already exists)
            //We also need to remove any special characters from the filename before attempting to create it           
            var series = seriesResource.InjectTo<Core.Tv.Series>();
            _seriesService.AddSeries(series);
            return series.InjectTo<SeriesResource>();
        }

        private SeriesResource UpdateSeries(SeriesResource seriesResource)
        {
            var series = _seriesService.GetSeries(seriesResource.Id);

            series.Monitored = seriesResource.Monitored;
            series.SeasonFolder = seriesResource.SeasonFolder;
            series.QualityProfileId = seriesResource.QualityProfileId;

            //Todo: Do we want to force a scan when this path changes? Can we use events instead?
            series.RootFolderId = seriesResource.RootFolderId;
            series.FolderName = seriesResource.FolderName;

            series.BacklogSetting = seriesResource.BacklogSetting;

            _seriesService.UpdateSeries(series);

            return series.InjectTo<SeriesResource>();
        }

        private void DeleteSeries(int id)
        {
            var deleteFiles = Convert.ToBoolean(Request.Headers["deleteFiles"].FirstOrDefault());
            _seriesService.DeleteSeries(id, deleteFiles);
        }
    }

}
