using System.Collections.Generic;
using System.Net;
using NzbDrone.Api.Episodes;
using NzbDrone.Api.Series;
using RestSharp;

namespace NzbDrone.Integration.Test.Client
{
    public class EpisodeClient : ClientBase<EpisodeResource>
    {
        public EpisodeClient(IRestClient restClient)
            : base(restClient, "episodes")
        {
        }

        public List<EpisodeResource> GetEpisodesInSeries(int seriesId)
        {
            var request = BuildRequest("?seriesId=" + seriesId.ToString());
            return Get<List<EpisodeResource>>(request);
        }
    }
}
