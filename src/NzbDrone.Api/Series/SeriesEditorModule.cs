using System.Collections.Generic;
using Nancy;
using NzbDrone.Api.Extensions;
using NzbDrone.Api.Mapping;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.Series
{
    public class SeriesEditorModule : NzbDroneApiModule
    {
        private readonly ISeriesService _seriesService;

        public SeriesEditorModule(ISeriesService seriesService)
            : base("/series/editor")
        {
            _seriesService = seriesService;
            Put["/"] = series => SaveAll();
        }

        private Response SaveAll()
        {
            //Read from request
            var series = Request.Body.FromJson<List<SeriesResource>>().InjectTo<List<Core.Tv.Series>>();

            return _seriesService.UpdateSeries(series)
                                 .InjectTo<List<SeriesResource>>()
                                 .AsResponse(HttpStatusCode.Accepted);
        }
    }
}
