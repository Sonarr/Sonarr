using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AutoMapper;
using FluentValidation;
using Nancy;
using NzbDrone.Api.Extentions;
using NzbDrone.Common;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;

namespace NzbDrone.Api.Series
{
    public class SeriesModule : NzbDroneApiModule
    {
        private readonly SeriesProvider _seriesProvider;
        private readonly JobProvider _jobProvider;

        public SeriesModule(SeriesProvider seriesProvider, JobProvider jobProvider)
            : base("/Series")
        {
            _seriesProvider = seriesProvider;
            _jobProvider = jobProvider;
            Get["/"] = x => AllSeries();
            Get["/{id}"] = x => GetSeries((int)x.id);
            Post["/"] = x => AddSeries();
            Put["/"] = x => UpdateSeries();

            Delete["/{id}"] = x => DeleteSeries((int)x.id);
        }

        private Response AllSeries()
        {
            var series = _seriesProvider.GetAllSeriesWithEpisodeCount().ToList();
            var seriesModels = Mapper.Map<List<Core.Repository.Series>, List<SeriesResource>>(series);

            return seriesModels.AsResponse();
        }

        private Response GetSeries(int id)
        {
            var series = _seriesProvider.GetSeries(id);
            var seriesModels = Mapper.Map<Core.Repository.Series, SeriesResource>(series);

            return seriesModels.AsResponse();
        }

        private Response AddSeries()
        {
            var request = Request.Body.FromJson<Core.Repository.Series>();

            //Todo: Alert the user if this series already exists
            //Todo: We need to create the folder if the user is adding a new series
            //(we can just create the folder and it won't blow up if it already exists)
            //We also need to remove any special characters from the filename before attempting to create it           

            _seriesProvider.AddSeries("", request.Path, request.SeriesId, request.QualityProfileId, null);
            _jobProvider.QueueJob(typeof(ImportNewSeriesJob));

            return new Response { StatusCode = HttpStatusCode.Created };
        }

        private Response UpdateSeries()
        {
            var request = Request.Body.FromJson<SeriesResource>();

            var series = _seriesProvider.GetSeries(request.Id);

            series.Monitored = request.Monitored;
            series.SeasonFolder = request.SeasonFolder;
            series.QualityProfileId = request.QualityProfileId;

            var oldPath = series.Path;

            series.Path = request.Path;
            series.BacklogSetting = (BacklogSettingType)request.BacklogSetting;

            if (!String.IsNullOrWhiteSpace(request.CustomStartDate))
                series.CustomStartDate = DateTime.Parse(request.CustomStartDate, null, DateTimeStyles.RoundtripKind);

            else
                series.CustomStartDate = null;

            _seriesProvider.UpdateSeries(series);

            if (oldPath != series.Path)
                _jobProvider.QueueJob(typeof(DiskScanJob), new { SeriesId = series.SeriesId });

            _seriesProvider.UpdateSeries(series);

            return request.AsResponse();
        }

        private Response DeleteSeries(int id)
        {
            var deleteFiles = Convert.ToBoolean(Request.Headers["deleteFiles"].FirstOrDefault());
            _jobProvider.QueueJob(typeof(DeleteSeriesJob), new { SeriesId = id, DeleteFiles = deleteFiles });
            return new Response { StatusCode = HttpStatusCode.OK };
        }
    }

    public class SeriesValidator : AbstractValidator<Core.Repository.Series>
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
                    RuleFor(s => s.SeriesId).GreaterThan(0);
                    RuleFor(s => s.Path).NotEmpty().Must(_diskProvider.FolderExists);
                    RuleFor(s => s.QualityProfileId).GreaterThan(0);
                });
        }
    }
}