using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AutoMapper;
using FluentValidation;
using NzbDrone.Api.Extensions;
using NzbDrone.Common;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Api.Validation;

namespace NzbDrone.Api.Series
{
    public class SeriesModule : NzbDroneRestModule<SeriesResource>
    {
        private readonly ISeriesService _seriesService;

        public SeriesModule(ISeriesService seriesService)
            : base("/Series")
        {
            _seriesService = seriesService;

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
            var seriesStats = _seriesService.SeriesStatistics();
            var seriesModels = Mapper.Map<List<Core.Tv.Series>, List<SeriesResource>>(series);

            foreach (var s in seriesModels)
            {
                var stats = seriesStats.SingleOrDefault(ss => ss.SeriesId == s.Id);
                if (stats == null) continue;

                s.EpisodeCount = stats.EpisodeCount;
                s.EpisodeFileCount = stats.EpisodeFileCount;
                s.NumberOfSeasons = stats.NumberOfSeasons;
                s.NextAiring = stats.NextAiring;
            }

            return seriesModels;
        }

        private SeriesResource GetSeries(int id)
        {
            var series = _seriesService.GetSeries(id);
            var seriesModels = Mapper.Map<Core.Tv.Series, SeriesResource>(series);
            return seriesModels;
        }

        private SeriesResource AddSeries(SeriesResource seriesResource)
        {
            var newSeries = Request.Body.FromJson<Core.Tv.Series>();

            //Todo: Alert the user if this series already exists
            //Todo: We need to create the folder if the user is adding a new series
            //(we can just create the folder and it won't blow up if it already exists)
            //We also need to remove any special characters from the filename before attempting to create it           
            var series = Mapper.Map<SeriesResource, Core.Tv.Series>(seriesResource);
            _seriesService.AddSeries(series);
            return Mapper.Map<Core.Tv.Series, SeriesResource>(series);
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

            series.BacklogSetting = (BacklogSettingType)seriesResource.BacklogSetting;

            if (!String.IsNullOrWhiteSpace(seriesResource.CustomStartDate))
                series.CustomStartDate = DateTime.Parse(seriesResource.CustomStartDate, null, DateTimeStyles.RoundtripKind);

            else
                series.CustomStartDate = null;

            _seriesService.UpdateSeries(series);

            return Mapper.Map<Core.Tv.Series, SeriesResource>(series);
        }

        private void DeleteSeries(int id)
        {
            var deleteFiles = Convert.ToBoolean(Request.Headers["deleteFiles"].FirstOrDefault());
            _seriesService.DeleteSeries(id, deleteFiles);
        }
    }

    public class SeriesValidator : AbstractValidator<Core.Tv.Series>
    {
        private readonly DiskProvider _diskProvider;

        public SeriesValidator(DiskProvider diskProvider)
        {
            _diskProvider = diskProvider;
        }

        public SeriesValidator()
        {
            RuleSet("POST", () =>
                {
                    RuleFor(s => s.Id).GreaterThan(0);
                    RuleFor(s => s.Path).NotEmpty().Must(_diskProvider.FolderExists);
                    RuleFor(s => s.QualityProfileId).GreaterThan(0);
                });
        }
    }
}