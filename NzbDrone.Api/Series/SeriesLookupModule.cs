using System.Linq;
using Nancy;
using NzbDrone.Api.Extensions;
using NzbDrone.Api.QualityType;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Providers;

namespace NzbDrone.Api.Series
{
    public class SeriesLookupModule : NzbDroneApiModule
    {
        private readonly TvDbProxy _tvDbProxy;

        public SeriesLookupModule(TvDbProxy tvDbProxy)
            : base("/Series/lookup")
        {
            _tvDbProxy = tvDbProxy;
            Get["/"] = x => GetQualityType();
        }


        private Response GetQualityType()
        {
            var tvDbResults = _tvDbProxy.SearchSeries((string)Request.Query.term);
            return tvDbResults.AsResponse();
        }
    }
}