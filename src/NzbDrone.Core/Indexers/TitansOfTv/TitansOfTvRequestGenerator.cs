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
        private const string BASE_API_URL = "https://titansof.tv/api/torrents";


        public IList<IEnumerable<IndexerRequest>> GetRecentRequests()
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            var innerList = new List<IndexerRequest>();

            var httpRequest = BuildHttpRequest(BASE_API_URL);

            innerList.Add(new IndexerRequest(httpRequest));
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
            
            const string url = BASE_API_URL + "?series_id={series}&episode={episode}";

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
            const string url = BASE_API_URL + "?series_id={series}&season={episode}";

            var requests = new List<IEnumerable<IndexerRequest>>();
            var innerList = new List<IndexerRequest>();
            requests.Add(innerList);

            var httpRequest = BuildHttpRequest(url);
            var seasonString = String.Format("Season {0:00}", searchCriteria.SeasonNumber);
            httpRequest.AddSegment("series", searchCriteria.Series.TvdbId.ToString(CultureInfo.InvariantCulture));
            httpRequest.AddSegment("season", seasonString);

            var request = new IndexerRequest(httpRequest);
            innerList.Add(request);

            httpRequest = BuildHttpRequest(url);
            seasonString = String.Format("Season {0}", searchCriteria.SeasonNumber);
            httpRequest.AddSegment("series", searchCriteria.Series.TvdbId.ToString(CultureInfo.InvariantCulture));
            httpRequest.AddSegment("season", seasonString);

            request = new IndexerRequest(httpRequest);
            innerList.Add(request);


            return requests;
        }

        public IList<IEnumerable<IndexerRequest>> GetSearchRequests(IndexerSearch.Definitions.DailyEpisodeSearchCriteria searchCriteria)
        {
            const string url = BASE_API_URL + "?series_id={series}&air_date={air_date}";

            var requests = new List<IEnumerable<IndexerRequest>>();
            var innerList = new List<IndexerRequest>();
            requests.Add(innerList);

            var httpRequest = BuildHttpRequest(url);
            var airDate = searchCriteria.AirDate.ToString("yyyy-MM-dd");
            httpRequest.AddSegment("series", searchCriteria.Series.TvdbId.ToString(CultureInfo.InvariantCulture));
            httpRequest.AddSegment("air_date", airDate);

            var request = new IndexerRequest(httpRequest);
            innerList.Add(request);


            return requests;
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
