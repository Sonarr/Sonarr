using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;
using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Indexers.GetStrike
{
    public class GetStrikeRequestGenerator : IIndexerRequestGenerator
    {
        public GetStrikeSettings Settings { get; set; }

        public IList<IEnumerable<IndexerRequest>> GetRecentRequests()
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            pageableRequests.AddIfNotNull(GetRequests("/torrents/top",
                "TV"));

            return pageableRequests;

        }

        public IList<IEnumerable<IndexerRequest>> GetSearchRequests(AnimeEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            foreach (var queryTitle in searchCriteria.QueryTitles)
            {
                pageableRequests.AddIfNotNull(GetRequests("/torrents/search",
                    "Anime",
                    PrepareQuery(queryTitle, String.Format("{0:00}", searchCriteria.AbsoluteEpisodeNumber))));
            }

            return pageableRequests;
        }

        public IList<IEnumerable<IndexerRequest>> GetSearchRequests(SpecialEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            foreach (var queryTitle in searchCriteria.EpisodeQueryTitles)
            {
                pageableRequests.AddIfNotNull(GetRequests("/torrents/search",
                    "TV",
                    PrepareQuery(queryTitle)));
            }

            return pageableRequests;
        }

        public IList<IEnumerable<IndexerRequest>> GetSearchRequests(DailyEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            foreach (var queryTitle in searchCriteria.QueryTitles)
            {
                pageableRequests.AddIfNotNull(GetRequests("/torrents/search",
                    "TV",
                    PrepareQuery(queryTitle, String.Format("{0:yyyy-MM-dd}", searchCriteria.AirDate))));

            }

            return pageableRequests;
        }

        public IList<IEnumerable<IndexerRequest>> GetSearchRequests(SeasonSearchCriteria searchCriteria)
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            foreach (var queryTitle in searchCriteria.QueryTitles)
            {
                pageableRequests.AddIfNotNull(GetRequests("/torrents/search",
                    "TV",
                    PrepareQuery(queryTitle, String.Format("S{0:00}", searchCriteria.SeasonNumber))));
            }

            return pageableRequests;
        }

        public IList<IEnumerable<IndexerRequest>> GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            foreach (var queryTitle in searchCriteria.QueryTitles)
            {
                pageableRequests.AddIfNotNull(GetRequests("/torrents/search",
                    "TV",
                    PrepareQuery(queryTitle, String.Format("S{0:00}E{1:00}", searchCriteria.SeasonNumber, searchCriteria.EpisodeNumber))));
            }

            return pageableRequests;
        }

        private String PrepareQuery(params String[] query)
        {
            return String.Join(" ", query).Replace("+", " ");
        }

        private IEnumerable<IndexerRequest> GetRequests(String path, String category, String phrase = null)
        {
            var builder = new HttpRequestBuilder(Settings.BaseUrl);
            HttpRequest request = builder.Build(path);
            request.Method = HttpMethod.GET;
            if (phrase != null)
            request.AddQueryParam("phrase", phrase);
            request.AddQueryParam("category", category);
            request.AllowAutoRedirect = true;

            yield return new IndexerRequest(request);
         }       
    }
}
