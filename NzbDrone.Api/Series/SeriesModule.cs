using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FluentValidation;
using Nancy;
using NzbDrone.Api.Extentions;
using NzbDrone.Api.QualityProfiles;
using NzbDrone.Common;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;

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
            Delete["/{id}"] = x => DeleteSeries((int)x.id);
        }

        private Response AllSeries()
        {
            var series = _seriesProvider.GetAllSeriesWithEpisodeCount().ToList();
            var seriesModels = Mapper.Map<List<Core.Repository.Series>, List<SeriesModel>>(series);

            return seriesModels.AsResponse();
        }

        private Response GetSeries(int id)
        {
            var series = _seriesProvider.GetSeries(id);
            var seriesModels = Mapper.Map<Core.Repository.Series, SeriesModel>(series);

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

        private Response DeleteSeries(int id)
        {
            //_seriesProvider.DeleteSeries(id);
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