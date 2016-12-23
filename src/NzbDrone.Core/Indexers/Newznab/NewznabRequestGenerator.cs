using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.Newznab
{
    public class NewznabRequestGenerator : IIndexerRequestGenerator
    {
        private readonly INewznabCapabilitiesProvider _capabilitiesProvider;
        public int MaxPages { get; set; }
        public int PageSize { get; set; }
        public NewznabSettings Settings { get; set; }

        public NewznabRequestGenerator(INewznabCapabilitiesProvider capabilitiesProvider)
        {
            _capabilitiesProvider = capabilitiesProvider;

            MaxPages = 30;
            PageSize = 100;
        }

        private bool SupportsSearch
        {
            get
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                return capabilities.SupportedSearchParameters != null &&
                       capabilities.SupportedSearchParameters.Contains("q");
            }
        }

        private bool SupportsTvSearch
        {
            get
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                return capabilities.SupportedTvSearchParameters != null &&
                       capabilities.SupportedTvSearchParameters.Contains("q") &&
                       capabilities.SupportedTvSearchParameters.Contains("season") &&
                       capabilities.SupportedTvSearchParameters.Contains("ep");
            }
        }

        private bool SupportsTvdbSearch
        {
            get
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                return capabilities.SupportedTvSearchParameters != null &&
                       capabilities.SupportedTvSearchParameters.Contains("tvdbid") &&
                       capabilities.SupportedTvSearchParameters.Contains("season") &&
                       capabilities.SupportedTvSearchParameters.Contains("ep");
            }
        }

        private bool SupportsTvRageSearch
        {
            get
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                return capabilities.SupportedTvSearchParameters != null &&
                       capabilities.SupportedTvSearchParameters.Contains("rid") &&
                       capabilities.SupportedTvSearchParameters.Contains("season") &&
                       capabilities.SupportedTvSearchParameters.Contains("ep");
            }
        }

        private bool SupportsTvMazeSearch
        {
            get
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                return capabilities.SupportedTvSearchParameters != null &&
                       capabilities.SupportedTvSearchParameters.Contains("tvmazeid") &&
                       capabilities.SupportedTvSearchParameters.Contains("season") &&
                       capabilities.SupportedTvSearchParameters.Contains("ep");
            }
        }

        private bool SupportsAggregatedIdSearch
        {
            get
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                return capabilities.SupportsAggregateIdSearch;
            }
        }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

            if (capabilities.SupportedTvSearchParameters != null)
            {
                pageableRequests.Add(GetPagedRequests(MaxPages, Settings.Categories.Concat(Settings.AnimeCategories), "tvsearch", ""));
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            AddTvIdPageableRequests(pageableRequests, MaxPages, Settings.Categories, searchCriteria,
                string.Format("&season={0}&ep={1}",
                    searchCriteria.SeasonNumber,
                    searchCriteria.EpisodeNumber));

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SeasonSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            AddTvIdPageableRequests(pageableRequests, MaxPages, Settings.Categories, searchCriteria,
                string.Format("&season={0}",
                    searchCriteria.SeasonNumber));

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(DailyEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            AddTvIdPageableRequests(pageableRequests, MaxPages, Settings.Categories, searchCriteria,
                string.Format("&season={0:yyyy}&ep={0:MM}/{0:dd}",
                searchCriteria.AirDate));

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(AnimeEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            if (SupportsSearch)
            {
                foreach (var queryTitle in searchCriteria.QueryTitles)
                {
                    pageableRequests.Add(GetPagedRequests(MaxPages, Settings.AnimeCategories, "search",
                        string.Format("&q={0}+{1:00}",
                        NewsnabifyTitle(queryTitle),
                        searchCriteria.AbsoluteEpisodeNumber)));
                }
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SpecialEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            if (SupportsSearch)
            {
                foreach (var queryTitle in searchCriteria.EpisodeQueryTitles)
                {
                    var query = queryTitle.Replace('+', ' ');
                    query = System.Web.HttpUtility.UrlEncode(query);

                    pageableRequests.Add(GetPagedRequests(MaxPages, Settings.Categories.Concat(Settings.AnimeCategories), "search",
                        string.Format("&q={0}",
                        query)));
                }
            }

            return pageableRequests;
        }

        private void AddTvIdPageableRequests(IndexerPageableRequestChain chain, int maxPages, IEnumerable<int> categories, SearchCriteriaBase searchCriteria, string parameters)
        {
            var includeTvdbSearch = SupportsTvdbSearch && searchCriteria.Series.TvdbId > 0;
            var includeTvRageSearch = SupportsTvRageSearch && searchCriteria.Series.TvRageId > 0;
            var includeTvMazeSearch = SupportsTvMazeSearch && searchCriteria.Series.TvMazeId > 0;

            if (SupportsAggregatedIdSearch && (includeTvdbSearch || includeTvRageSearch || includeTvMazeSearch))
            {
                var ids = "";

                if (includeTvdbSearch)
                {
                    ids += "&tvdbid=" + searchCriteria.Series.TvdbId;
                }

                if (includeTvRageSearch)
                {
                    ids += "&rid=" + searchCriteria.Series.TvRageId;
                }

                if (includeTvMazeSearch)
                {
                    ids += "&tvmazeid=" + searchCriteria.Series.TvMazeId;
                }

                chain.Add(GetPagedRequests(maxPages, categories, "tvsearch", ids + parameters));
            }
            else
            {
                if (includeTvdbSearch)
                {
                    chain.Add(GetPagedRequests(maxPages, categories, "tvsearch",
                        string.Format("&tvdbid={0}{1}", searchCriteria.Series.TvdbId, parameters)));
                }
                else if (includeTvRageSearch)
                {
                    chain.Add(GetPagedRequests(maxPages, categories, "tvsearch",
                        string.Format("&rid={0}{1}", searchCriteria.Series.TvRageId, parameters)));
                }

                else if (includeTvMazeSearch)
                {
                    chain.Add(GetPagedRequests(maxPages, categories, "tvsearch",
                        string.Format("&tvmazeid={0}{1}", searchCriteria.Series.TvMazeId, parameters)));
                }
            }

            if (SupportsTvSearch)
            {
                chain.AddTier();
                foreach (var queryTitle in searchCriteria.QueryTitles)
                {
                    chain.Add(GetPagedRequests(MaxPages, Settings.Categories, "tvsearch",
                        string.Format("&q={0}{1}",
                        NewsnabifyTitle(queryTitle),
                        parameters)));
                }
            }
        }

        private IEnumerable<IndexerRequest> GetPagedRequests(int maxPages, IEnumerable<int> categories, string searchType, string parameters)
        {
            if (categories.Empty())
            {
                yield break;
            }

            var categoriesQuery = string.Join(",", categories.Distinct());

            var baseUrl = string.Format("{0}/api?t={1}&cat={2}&extended=1{3}", Settings.Url.TrimEnd('/'), searchType, categoriesQuery, Settings.AdditionalParameters);

            if (Settings.ApiKey.IsNotNullOrWhiteSpace())
            {
                baseUrl += "&apikey=" + Settings.ApiKey;
            }

            if (PageSize == 0)
            {
                yield return new IndexerRequest(string.Format("{0}{1}", baseUrl, parameters), HttpAccept.Rss);
            }
            else
            {
                for (var page = 0; page < maxPages; page++)
                {
                    yield return new IndexerRequest(string.Format("{0}&offset={1}&limit={2}{3}", baseUrl, page * PageSize, PageSize, parameters), HttpAccept.Rss);
                }
            }
        }

        private static string NewsnabifyTitle(string title)
        {
            return title.Replace("+", "%20");
        }
    }
}
