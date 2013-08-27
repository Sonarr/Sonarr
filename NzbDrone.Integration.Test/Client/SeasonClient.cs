using System.Collections.Generic;
using System.Net;
using NzbDrone.Api.Episodes;
using NzbDrone.Api.Seasons;
using RestSharp;

namespace NzbDrone.Integration.Test.Client
{
    public class SeasonClient : ClientBase<SeasonResource>
    {
        public SeasonClient(IRestClient restClient)
            : base(restClient)
        {
        }

        public List<SeasonResource> GetSeasonsInSeries(int seriesId)
        {
            var request = BuildRequest("?seriesId=" + seriesId.ToString());
            return Get<List<SeasonResource>>(request);
        }
    }
}
