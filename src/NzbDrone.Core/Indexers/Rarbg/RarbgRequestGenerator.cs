using System;
using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.Rarbg
{
    public class RarbgRequestGenerator : IIndexerRequestGenerator
    {
        private readonly IRarbgTokenProvider _tokenProvider;

        public RarbgSettings Settings { get; set; }

        public RarbgRequestGenerator(IRarbgTokenProvider tokenProvider)
        {
            _tokenProvider = tokenProvider;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetRecentRequests()
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            pageableRequests.AddIfNotNull(GetPagedRequests("list", null, null));

            return pageableRequests;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            pageableRequests.AddIfNotNull(GetPagedRequests("search", searchCriteria.Series.TvdbId, "S{0:00}E{1:00}", searchCriteria.SeasonNumber, searchCriteria.EpisodeNumber));

            return pageableRequests;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(SeasonSearchCriteria searchCriteria)
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            pageableRequests.AddIfNotNull(GetPagedRequests("search", searchCriteria.Series.TvdbId, "S{0:00}", searchCriteria.SeasonNumber));

            return pageableRequests;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(DailyEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            pageableRequests.AddIfNotNull(GetPagedRequests("search", searchCriteria.Series.TvdbId, "\"{0:yyyy MM dd}\"", searchCriteria.AirDate));

            return pageableRequests;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(AnimeEpisodeSearchCriteria searchCriteria)
        {
            return new List<IEnumerable<IndexerRequest>>();
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(SpecialEpisodeSearchCriteria searchCriteria)
        {
            return new List<IEnumerable<IndexerRequest>>();
        }

        private IEnumerable<IndexerRequest> GetPagedRequests(string mode, int? tvdbId, string query, params object[] args)
        {
            var httpRequest = new HttpRequest(Settings.BaseUrl + "/pubapi_v2.php", HttpAccept.Json);

            httpRequest.AddQueryParam("mode", mode);

            if (tvdbId.HasValue)
            {
                httpRequest.AddQueryParam("search_tvdb", tvdbId.Value.ToString());
            }

            if (query.IsNotNullOrWhiteSpace())
            {
                httpRequest.AddQueryParam("search_string", string.Format(query, args));
            }

            if (!Settings.RankedOnly)
            {
                httpRequest.AddQueryParam("ranked", "0");
            }

            httpRequest.AddQueryParam("category", "18;41");
            httpRequest.AddQueryParam("limit", "100");
            httpRequest.AddQueryParam("token", _tokenProvider.GetToken(Settings));
            httpRequest.AddQueryParam("format", "json_extended");
            httpRequest.AddQueryParam("app_id", "Sonarr");

            yield return new IndexerRequest(httpRequest);
        }
    }
}
