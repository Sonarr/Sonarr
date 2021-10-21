using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.Tribler
{
    public class TriblerIndexerRequestGenerator : IIndexerRequestGenerator
    {
        public TriblerIndexerSettings Settings { get; set; }

        // Tribler currently has no rss support.
        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetRequest("*")); // search for anything

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(AnimeEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            foreach (var episode in searchCriteria.Episodes)
            {
                var query = string.Format("{0} S{1:00}E{2:00}", searchCriteria.Series.Title, episode.SeasonNumber, episode.EpisodeNumber);
                pageableRequests.Add(GetRequest(query));
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SpecialEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            // not sure if this is the correct way to handle special episodes, it's mostly copy-paste.
            var episodeQueryTitle = searchCriteria.EpisodeQueryTitles.Where(e => !string.IsNullOrWhiteSpace(e))
                   .Select(e => SearchCriteriaBase.GetCleanSceneTitle(e))
                   .ToArray();

            foreach (var queryTitle in episodeQueryTitle)
            {
                var query = queryTitle.Replace('+', ' ');
                query = System.Web.HttpUtility.UrlEncode(query);

                pageableRequests.Add(GetRequest(query));
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(DailyEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            var query = string.Format("{0} {1:yyyy}.{1:MM}.{1:dd}", searchCriteria.Series.Title, searchCriteria.AirDate);

            pageableRequests.Add(GetRequest(query));

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(DailySeasonSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            var query = string.Format("{0} {1}", searchCriteria.Series.Title, searchCriteria.Year);

            pageableRequests.Add(GetRequest(query));

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SeasonSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            foreach (var seasonNumber in searchCriteria.Episodes.Select(e => e.SeasonNumber).Distinct())
            {
                var query = string.Format("{0} S{1:00}E", searchCriteria.Series.Title, seasonNumber);
                pageableRequests.Add(GetRequest(query));

                query = string.Format("{0} Season {1}", searchCriteria.Series.Title, seasonNumber);
                pageableRequests.Add(GetRequest(query));
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            foreach (var episode in searchCriteria.Episodes)
            {
                var query = string.Format("{0} S{1:00}E{2:00}", searchCriteria.Series.Title, episode.SeasonNumber, episode.EpisodeNumber);

                pageableRequests.Add(GetRequest(query));
            }

            return pageableRequests;
        }

        public IEnumerable<IndexerRequest> GetRequest(string query)
        {
            var requestBuilder = new HttpRequestBuilder(HttpUri.CombinePath(Settings.BaseUrl, "search"))
                .Accept(HttpAccept.Json);

            requestBuilder.Headers.Add("X-Api-Key", Settings.ApiKey);

            requestBuilder.LogResponseContent = true;

            requestBuilder.AddQueryParam("txt_filter", query);
            requestBuilder.AddQueryParam("metadata_type", "torrent"); // otherwise tribler channels should be returned as part of the response.

            // todo: should probably page the requests here with indexes etc.

            yield return new IndexerRequest(requestBuilder.Build());
        }

    }
}
