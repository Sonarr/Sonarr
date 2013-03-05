using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AutoMapper;
using FluentValidation;
using Nancy;
using NzbDrone.Api.Extensions;
using NzbDrone.Common;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Jobs.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Model;

namespace NzbDrone.Api.Series
{
    public class SeriesModule : NzbDroneApiModule
    {
        private readonly ISeriesService _seriesService;
        private readonly ISeriesRepository _seriesRepository;
        private readonly IJobController _jobProvider;

        public SeriesModule(ISeriesService seriesService,ISeriesRepository seriesRepository, IJobController jobProvider)
            : base("/Series")
        {
            _seriesService = seriesService;
            _seriesRepository = seriesRepository;
            _jobProvider = jobProvider;
            Get["/"] = x => AllSeries();
            Get["/{id}"] = x => GetSeries((int)x.id);
            Post["/"] = x => AddSeries();
            Put["/"] = x => UpdateSeries();

            Delete["/{id}"] = x => DeleteSeries((int)x.id);
        }

        private Response AllSeries()
        {
            var series = _seriesRepository.All().ToList();
            var seriesModels = Mapper.Map<List<Core.Tv.Series>, List<SeriesResource>>(series);

            return seriesModels.AsResponse();
        }

        private Response GetSeries(int id)
        {
            var series = _seriesRepository.Get(id);
            var seriesModels = Mapper.Map<Core.Tv.Series, SeriesResource>(series);

            return seriesModels.AsResponse();
        }

        private Response AddSeries()
        {
            var request = Request.Body.FromJson<Core.Tv.Series>();

            //Todo: Alert the user if this series already exists
            //Todo: We need to create the folder if the user is adding a new series
            //(we can just create the folder and it won't blow up if it already exists)
            //We also need to remove any special characters from the filename before attempting to create it           

            _seriesService.AddSeries(request.Title, request.Path, request.TvDbId, request.QualityProfileId, null);
            _jobProvider.Enqueue(typeof(ImportNewSeriesJob));

            return new Response { StatusCode = HttpStatusCode.Created };
        }

        private Response UpdateSeries()
        {
            var request = Request.Body.FromJson<SeriesResource>();

            var series = _seriesRepository.Get(request.Id);

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

            _seriesRepository.Update(series);

            if (oldPath != series.Path)
                _jobProvider.Enqueue(typeof(DiskScanJob), new { SeriesId = series.Id });

            _seriesRepository.Update(series);

            return request.AsResponse();
        }

        private Response DeleteSeries(int id)
        {
            var deleteFiles = Convert.ToBoolean(Request.Headers["deleteFiles"].FirstOrDefault());
            _jobProvider.Enqueue(typeof(DeleteSeriesJob), new { SeriesId = id, DeleteFiles = deleteFiles });
            return new Response { StatusCode = HttpStatusCode.OK };
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
                    RuleFor(s => ((ModelBase)s).Id).GreaterThan(0);
                    RuleFor(s => s.Path).NotEmpty().Must(_diskProvider.FolderExists);
                    RuleFor(s => s.QualityProfileId).GreaterThan(0);
                });
        }
    }
}