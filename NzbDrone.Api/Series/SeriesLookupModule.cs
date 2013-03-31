using Nancy;
using NzbDrone.Api.Extensions;
using NzbDrone.Core.MetadataSource;

namespace NzbDrone.Api.Series
{
    public class SeriesLookupModule : NzbDroneApiModule
    {
        private readonly ISearchForNewSeries _searchProxy;

        public SeriesLookupModule(ISearchForNewSeries searchProxy)
            : base("/Series/lookup")
        {
            _searchProxy = searchProxy;
            Get["/"] = x => GetQualityType();
        }


        private Response GetQualityType()
        {
            var tvDbResults = _searchProxy.SearchForNewSeries((string)Request.Query.term);
            return tvDbResults.AsResponse();
        }
    }
}