using System.Collections.Generic;
using RestSharp;
using Sonarr.Api.V3.Episodes;

namespace NzbDrone.Integration.Test.Client
{
    public class EpisodeClient : ClientBase<EpisodeResource>
    {
        public EpisodeClient(IRestClient restClient, string apiKey)
            : base(restClient, apiKey, "episode")
        {
        }

        public List<EpisodeResource> GetEpisodesInSeries(int seriesId)
        {
            var request = BuildRequest("?seriesId=" + seriesId.ToString());
            return Get<List<EpisodeResource>>(request);
        }

        public EpisodeResource SetMonitored(EpisodeResource episode)
        {
            var request = BuildRequest(episode.Id.ToString());
            request.AddJsonBody(episode);
            return Put<EpisodeResource>(request);
        }
    }
}
