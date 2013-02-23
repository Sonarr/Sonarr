using System.Linq;
using Nancy;
using NzbDrone.Api.Extensions;
using NzbDrone.Api.QualityType;
using NzbDrone.Core.Providers;

namespace NzbDrone.Api.Series
{
    public class SeriesLookupModule : NzbDroneApiModule
    {
        private readonly TvDbProvider _tvDbProvider;

        public SeriesLookupModule(TvDbProvider tvDbProvider)
            : base("/Series/lookup")
        {
            _tvDbProvider = tvDbProvider;
            Get["/"] = x => GetQualityType();
        }


        private Response GetQualityType()
        {
            var tvDbResults = _tvDbProvider.SearchSeries((string)Request.Query.term);
            return tvDbResults.AsResponse();
        }
    }
}