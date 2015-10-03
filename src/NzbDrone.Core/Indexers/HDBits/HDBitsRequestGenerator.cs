using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.HDBits
{
    public class HDBitsRequestGenerator : IIndexerRequestGenerator
    {
        public HDBitsSettings Settings { get; set; }

        public IList<IEnumerable<IndexerRequest>> GetRecentRequests()
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            pageableRequests.Add(GetRequest(new TorrentQuery()));

            return pageableRequests;
        }

        public IList<IEnumerable<IndexerRequest>> GetSearchRequests(AnimeEpisodeSearchCriteria searchCriteria)
        {
            var requests = new List<IEnumerable<IndexerRequest>>();

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

            return requests;
        }

        public IList<IEnumerable<IndexerRequest>> GetSearchRequests(SpecialEpisodeSearchCriteria searchCriteria)
        {
            return new List<IEnumerable<IndexerRequest>>();
        }

        public IList<IEnumerable<IndexerRequest>> GetSearchRequests(DailyEpisodeSearchCriteria searchCriteria)
        {
            var requests = new List<IEnumerable<IndexerRequest>>();

            var query = new TorrentQuery();
            if (TryAddSearchParameters(query, searchCriteria))
            {
                query.Search = string.Format("{0:yyyy}-{0:MM}-{0:dd}", searchCriteria.AirDate);

                requests.Add(GetRequest(query));
            }

            return requests;
        }

        public IList<IEnumerable<IndexerRequest>> GetSearchRequests(SeasonSearchCriteria searchCriteria)
        {
            var requests = new List<IEnumerable<IndexerRequest>>();

            var queryBase = new TorrentQuery();
            if (TryAddSearchParameters(queryBase, searchCriteria))
            {
                foreach (var seasonNumber in searchCriteria.Episodes.Select(e => e.SeasonNumber).Distinct())
                {
                    var query = queryBase.Clone();

                    query.TvdbInfo.Season = seasonNumber;

                    requests.Add(GetRequest(query));
                }
            }

            return requests;
        }

        public IList<IEnumerable<IndexerRequest>> GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
        {
            var requests = new List<IEnumerable<IndexerRequest>>();

            var queryBase = new TorrentQuery();
            if (TryAddSearchParameters(queryBase, searchCriteria))
            {
                foreach (var episode in searchCriteria.Episodes)
                {
                    var query = queryBase.Clone();

                    query.TvdbInfo.Season = episode.SeasonNumber;
                    query.TvdbInfo.Episode = episode.EpisodeNumber;

                    requests.Add(GetRequest(query));
                }
            }

            return requests;
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
            var builder = new HttpRequestBuilder(Settings.BaseUrl);
            var request = builder.Build("/api/torrents");

            request.Method = HttpMethod.POST;
            const string appJson = "application/json";
            request.Headers.Accept = appJson;
            request.Headers.ContentType = appJson;

            query.Username = Settings.Username;
            query.Passkey = Settings.ApiKey;

            request.Body = query.ToJson();

            yield return new IndexerRequest(request);
        }
    }
}
