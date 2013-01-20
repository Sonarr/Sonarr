using System.Linq;
using Nancy;
using NzbDrone.Api.QualityType;
using NzbDrone.Core.Providers;

namespace NzbDrone.Api.Series
{
    public class SeriesModule : NzbDroneApiModule
    {
        private readonly TvDbProvider _tvDbProvider;

        public SeriesModule(TvDbProvider tvDbProvider)
            : base("/Series")
        {
            _tvDbProvider = tvDbProvider;
            Get["/lookup"] = x => GetQualityType();
        }


        private Response GetQualityType()
        {
            var tvDbResults = _tvDbProvider.SearchSeries((string)Request.Query.term);
            return tvDbResults.AsResponse();
        }
    }
}