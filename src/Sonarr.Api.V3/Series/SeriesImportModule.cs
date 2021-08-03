using System.Collections.Generic;
using Nancy;
using NzbDrone.Core.Tv;
using Sonarr.Http;
using Sonarr.Http.Extensions;

namespace Sonarr.Api.V3.Series
{
    public class SeriesImportModule : SonarrRestModule<SeriesResource>
    {
        private readonly IAddSeriesService _addSeriesService;

        public SeriesImportModule(IAddSeriesService addSeriesService)
            : base("/series/import")
        {
            _addSeriesService = addSeriesService;
            Post("/",  x => Import());
        }

        private object Import()
        {
            var resource = Request.Body.FromJson<List<SeriesResource>>();
            var newSeries = resource.ToModel();

            return _addSeriesService.AddSeries(newSeries).ToResource();
        }
    }
}
