using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Tv;
using Sonarr.Http;

namespace Sonarr.Api.V5.Series
{
    [V5ApiController("series/import")]
    public class SeriesImportController : Controller
    {
        private readonly IAddSeriesService _addSeriesService;

        public SeriesImportController(IAddSeriesService addSeriesService)
        {
            _addSeriesService = addSeriesService;
        }

        [HttpPost]
        public object Import([FromBody] List<SeriesResource> resource)
        {
            var newSeries = resource.ToModel();

            return _addSeriesService.AddSeries(newSeries).ToResource();
        }
    }
}
