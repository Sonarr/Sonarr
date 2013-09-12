using System.Collections.Generic;
using NzbDrone.Api.Episodes;
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
