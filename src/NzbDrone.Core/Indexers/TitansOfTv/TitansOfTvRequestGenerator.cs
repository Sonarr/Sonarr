using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.Indexers.TitansOfTv
{
    public class TitansOfTvRequestGenerator : IIndexerRequestGenerator
    {
        public TitansOfTvSettings Settings { get; set; }

        private const string BASE_RSS_URL = "https://titansof.tv/rss/feed/?apikey={0}";
        private const string BASE_API_URL = "https://titansof.tv/api/torrents?";


        public IList<IEnumerable<IndexerRequest>> GetRecentRequests()
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();
            var url = string.Format(BASE_RSS_URL, Settings.ApiKey);
            var innerList = new List<IndexerRequest>();

            innerList.Add(new IndexerRequest(url, HttpAccept.Rss));
            pageableRequests.Add(innerList);

            return pageableRequests;
        }


        private HttpRequest BuildHttpRequest(string url)
        {


            var httpRequest = new HttpRequest(url, HttpAccept.Json);
            httpRequest.Headers["X-Authorization"] = Settings.ApiKey;
            return httpRequest;

        }

        public IList<IEnumerable<IndexerRequest>> GetSearchRequests(IndexerSearch.Definitions.SingleEpisodeSearchCriteria searchCriteria)
        {
            var json = JsonConvert.SerializeObject(searchCriteria, Formatting.Indented, new StringEnumConverter());
            const string url = BASE_API_URL + "series_id={series}&episode={episode}";

            var requests = new List<IEnumerable<IndexerRequest>>();
            var innerList = new List<IndexerRequest>();
            requests.Add(innerList);

            var httpRequest = BuildHttpRequest(url);
            var episodeString = String.Format("S{0:00}E{1:00}", searchCriteria.SeasonNumber, searchCriteria.EpisodeNumber);
            httpRequest.AddSegment("series", searchCriteria.Series.TvdbId.ToString(CultureInfo.InvariantCulture));
            httpRequest.AddSegment("episode", episodeString);

            var request = new IndexerRequest(httpRequest);
            innerList.Add(request);


            return requests;
        }

        public IList<IEnumerable<IndexerRequest>> GetSearchRequests(IndexerSearch.Definitions.SeasonSearchCriteria searchCriteria)
        {
            return new List<IEnumerable<IndexerRequest>>();
        }

        public IList<IEnumerable<IndexerRequest>> GetSearchRequests(IndexerSearch.Definitions.DailyEpisodeSearchCriteria searchCriteria)
        {
            return new List<IEnumerable<IndexerRequest>>();
        }

        public IList<IEnumerable<IndexerRequest>> GetSearchRequests(IndexerSearch.Definitions.AnimeEpisodeSearchCriteria searchCriteria)
        {
            return new List<IEnumerable<IndexerRequest>>();
        }

        public IList<IEnumerable<IndexerRequest>> GetSearchRequests(IndexerSearch.Definitions.SpecialEpisodeSearchCriteria searchCriteria)
        {
            return new List<IEnumerable<IndexerRequest>>();
        }
    }
}
