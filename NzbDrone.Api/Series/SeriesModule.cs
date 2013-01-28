using System.Linq;
using FluentValidation;
using Nancy;
using NzbDrone.Api.Extentions;
using NzbDrone.Common;
using NzbDrone.Core.Jobs;
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
            Post["/"] = x => AddSeries();
        }


        private Response AddSeries()
        {
            var request = Request.Body.FromJson<Core.Repository.Series>();

            _seriesProvider.AddSeries("", request.Path, request.SeriesId, request.QualityProfileId, null);
            _jobProvider.QueueJob(typeof(ImportNewSeriesJob));

            return new Response { StatusCode = HttpStatusCode.Created };
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