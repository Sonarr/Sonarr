using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.HDBits
{
    public class HDBitsRequestGenerator : IIndexerRequestGenerator
    {
        public HDBitsSettings Settings { get; set; }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetRequest(new TorrentQuery()));

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(AnimeEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            var queryBase = new TorrentQuery();
            if (TryAddSearchParameters(queryBase, searchCriteria))
            {
                foreach (var episode in searchCriteria.Episodes)
                {
                    var query = queryBase.Clone();

                    query.TvdbInfo.Season = episode.SeasonNumber;
                    query.TvdbInfo.Episode = episode.EpisodeNumber;
                }
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SpecialEpisodeSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(DailyEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            var query = new TorrentQuery();
            if (TryAddSearchParameters(query, searchCriteria))
            {
                query.Search = string.Format("{0:yyyy}-{0:MM}-{0:dd}", searchCriteria.AirDate);

                pageableRequests.Add(GetRequest(query));
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(DailySeasonSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            var query = new TorrentQuery();
            if (TryAddSearchParameters(query, searchCriteria))
            {
                query.Search = string.Format("{0}-", searchCriteria.Year);

                pageableRequests.Add(GetRequest(query));
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SeasonSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            var queryBase = new TorrentQuery();
            if (TryAddSearchParameters(queryBase, searchCriteria))
            {
                foreach (var seasonNumber in searchCriteria.Episodes.Select(e => e.SeasonNumber).Distinct())
                {
                    var query = queryBase.Clone();

                    query.TvdbInfo.Season = seasonNumber;

                    pageableRequests.Add(GetRequest(query));
                }
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            var queryBase = new TorrentQuery();
            if (TryAddSearchParameters(queryBase, searchCriteria))
            {
                foreach (var episode in searchCriteria.Episodes)
                {
                    var query = queryBase.Clone();

                    query.TvdbInfo.Season = episode.SeasonNumber;
                    query.TvdbInfo.Episode = episode.EpisodeNumber;

                    pageableRequests.Add(GetRequest(query));
                }
            }

            return pageableRequests;
        }

        private bool TryAddSearchParameters(TorrentQuery query, SearchCriteriaBase searchCriteria)
        {
            if (searchCriteria.Series.TvdbId != 0)
            {
                query.TvdbInfo = query.TvdbInfo ?? new TvdbInfo();
                query.TvdbInfo.Id = searchCriteria.Series.TvdbId;
                return true;
            }

            return false;
        }

        private IEnumerable<IndexerRequest> GetRequest(TorrentQuery query)
        {
            var request = new HttpRequestBuilder(Settings.BaseUrl)
                .Resource("/api/torrents")
                .Build();

            request.Method = HttpMethod.Post;
            const string appJson = "application/json";
            request.Headers.Accept = appJson;
            request.Headers.ContentType = appJson;

            query.Username = Settings.Username;
            query.Passkey = Settings.ApiKey;

            request.SetContent(query.ToJson());

            yield return new IndexerRequest(request);
        }
    }
}
