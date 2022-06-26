using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.Newznab
{
    public class NewznabRequestGenerator : IIndexerRequestGenerator
    {
        private readonly Logger _logger;
        private readonly INewznabCapabilitiesProvider _capabilitiesProvider;

        public int MaxPages { get; set; }
        public int PageSize { get; set; }
        public NewznabSettings Settings { get; set; }

        public NewznabRequestGenerator(INewznabCapabilitiesProvider capabilitiesProvider)
        {
            _logger = NzbDroneLogger.GetLogger(GetType());
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

        private bool SupportsTvTitleSearch
        {
            get
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                return capabilities.SupportedTvSearchParameters != null &&
                       capabilities.SupportedTvSearchParameters.Contains("title") &&
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

        private bool SupportsImdbSearch
        {
            get
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                return capabilities.SupportedTvSearchParameters != null &&
                       capabilities.SupportedTvSearchParameters.Contains("imdbid") &&
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

        private string TextSearchEngine
        {
            get
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                return capabilities.TextSearchEngine;
            }
        }

        private string TvTextSearchEngine
        {
            get
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                return capabilities.TvTextSearchEngine;
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
            else if (capabilities.SupportedSearchParameters != null)
            {
                pageableRequests.Add(GetPagedRequests(MaxPages, Settings.AnimeCategories, "search", ""));
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
        {
            if (!SupportsTvdbSearch && !SupportsTvSearch && !SupportsTvTitleSearch)
            {
                _logger.Debug("Indexer capabilities lacking season & ep query parameters, no Standard series search possible.");
            }

            var pageableRequests = new IndexerPageableRequestChain();

            if (searchCriteria.SearchMode.HasFlag(SearchMode.SearchID) || searchCriteria.SearchMode == SearchMode.Default)
            {
                AddTvIdPageableRequests(pageableRequests,
                    Settings.Categories,
                    searchCriteria,
                    string.Format("&season={0}&ep={1}",
                        NewznabifySeasonNumber(searchCriteria.SeasonNumber),
                        searchCriteria.EpisodeNumber));
            }

            if (searchCriteria.SearchMode.HasFlag(SearchMode.SearchTitle))
            {
                AddTitlePageableRequests(pageableRequests,
                    Settings.Categories,
                    searchCriteria,
                    string.Format("&season={0}&ep={1}",
                        NewznabifySeasonNumber(searchCriteria.SeasonNumber),
                        searchCriteria.EpisodeNumber));
            }

            pageableRequests.AddTier();

            if (searchCriteria.SearchMode == SearchMode.Default)
            {
                AddTitlePageableRequests(pageableRequests,
                    Settings.Categories,
                    searchCriteria,
                    string.Format("&season={0}&ep={1}",
                        NewznabifySeasonNumber(searchCriteria.SeasonNumber),
                        searchCriteria.EpisodeNumber));
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SeasonSearchCriteria searchCriteria)
        {
            if (!SupportsTvdbSearch && !SupportsTvSearch && !SupportsTvTitleSearch)
            {
                _logger.Debug("Indexer capabilities lacking season & ep query parameters, no Standard series search possible.");
            }

            var pageableRequests = new IndexerPageableRequestChain();

            if (searchCriteria.SearchMode.HasFlag(SearchMode.SearchID) || searchCriteria.SearchMode == SearchMode.Default)
            {
                AddTvIdPageableRequests(pageableRequests,
                    Settings.Categories,
                    searchCriteria,
                    string.Format("&season={0}",
                        NewznabifySeasonNumber(searchCriteria.SeasonNumber)));
            }

            if (searchCriteria.SearchMode.HasFlag(SearchMode.SearchTitle))
            {
                AddTitlePageableRequests(pageableRequests,
                    Settings.Categories,
                    searchCriteria,
                    string.Format("&season={0}",
                        NewznabifySeasonNumber(searchCriteria.SeasonNumber)));
            }

            pageableRequests.AddTier();

            if (searchCriteria.SearchMode == SearchMode.Default)
            {
                AddTitlePageableRequests(pageableRequests,
                    Settings.Categories,
                    searchCriteria,
                    string.Format("&season={0}",
                        NewznabifySeasonNumber(searchCriteria.SeasonNumber)));
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(DailyEpisodeSearchCriteria searchCriteria)
        {
            if (!SupportsTvdbSearch && !SupportsTvSearch && !SupportsTvTitleSearch)
            {
                _logger.Debug("Indexer capabilities lacking season & ep query parameters, no Daily series search possible.");
            }

            var pageableRequests = new IndexerPageableRequestChain();

            if (searchCriteria.SearchMode.HasFlag(SearchMode.SearchID) || searchCriteria.SearchMode == SearchMode.Default)
            {
                AddTvIdPageableRequests(pageableRequests,
                    Settings.Categories,
                    searchCriteria,
                    string.Format("&season={0:yyyy}&ep={0:MM}/{0:dd}",
                    searchCriteria.AirDate));
            }

            if (searchCriteria.SearchMode.HasFlag(SearchMode.SearchTitle))
            {
                AddTitlePageableRequests(pageableRequests,
                    Settings.Categories,
                    searchCriteria,
                    string.Format("&season={0:yyyy}&ep={0:MM}/{0:dd}",
                    searchCriteria.AirDate));
            }

            pageableRequests.AddTier();

            if (searchCriteria.SearchMode == SearchMode.Default)
            {
                AddTitlePageableRequests(pageableRequests,
                    Settings.Categories,
                    searchCriteria,
                    string.Format("&season={0:yyyy}&ep={0:MM}/{0:dd}",
                    searchCriteria.AirDate));
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(DailySeasonSearchCriteria searchCriteria)
        {
            if (!SupportsTvdbSearch && !SupportsTvSearch && !SupportsTvTitleSearch)
            {
                _logger.Debug("Indexer capabilities lacking season & ep query parameters, no Daily series search possible.");
            }

            var pageableRequests = new IndexerPageableRequestChain();

            if (searchCriteria.SearchMode.HasFlag(SearchMode.SearchID) || searchCriteria.SearchMode == SearchMode.Default)
            {
                AddTvIdPageableRequests(pageableRequests,
                    Settings.Categories,
                    searchCriteria,
                    string.Format("&season={0}",
                    searchCriteria.Year));
            }

            if (searchCriteria.SearchMode.HasFlag(SearchMode.SearchTitle))
            {
                AddTitlePageableRequests(pageableRequests,
                    Settings.Categories,
                    searchCriteria,
                    string.Format("&season={0}",
                    searchCriteria.Year));
            }

            pageableRequests.AddTier();

            if (searchCriteria.SearchMode == SearchMode.Default)
            {
                AddTitlePageableRequests(pageableRequests,
                    Settings.Categories,
                    searchCriteria,
                    string.Format("&season={0}",
                    searchCriteria.Year));
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(AnimeEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            if (SupportsSearch)
            {
                var queryTitles = TextSearchEngine == "raw" ? searchCriteria.SceneTitles : searchCriteria.CleanSceneTitles;
                foreach (var queryTitle in queryTitles)
                {
                    pageableRequests.Add(GetPagedRequests(MaxPages,
                        Settings.AnimeCategories,
                        "search",
                        string.Format("&q={0}+{1:00}",
                        NewsnabifyTitle(queryTitle),
                        searchCriteria.AbsoluteEpisodeNumber)));

                    if (Settings.AnimeStandardFormatSearch && searchCriteria.SeasonNumber > 0 && searchCriteria.EpisodeNumber > 0)
                    {
                        pageableRequests.Add(GetPagedRequests(MaxPages,
                            Settings.AnimeCategories,
                            "tvsearch",
                            string.Format("&q={0}&season={1}&ep={2}",
                            NewsnabifyTitle(queryTitle),
                            searchCriteria.SeasonNumber,
                            searchCriteria.EpisodeNumber)));
                    }
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

                    pageableRequests.Add(GetPagedRequests(MaxPages,
                        Settings.Categories.Concat(Settings.AnimeCategories),
                        "search",
                        string.Format("&q={0}",
                        query)));
                }
            }

            return pageableRequests;
        }

        private void AddTvIdPageableRequests(IndexerPageableRequestChain chain, IEnumerable<int> categories, SearchCriteriaBase searchCriteria, string parameters)
        {
            var includeTvdbSearch = SupportsTvdbSearch && searchCriteria.Series.TvdbId > 0;
            var includeImdbSearch = SupportsImdbSearch && searchCriteria.Series.ImdbId.IsNotNullOrWhiteSpace();
            var includeTvRageSearch = SupportsTvRageSearch && searchCriteria.Series.TvRageId > 0;
            var includeTvMazeSearch = SupportsTvMazeSearch && searchCriteria.Series.TvMazeId > 0;

            if (SupportsAggregatedIdSearch && (includeTvdbSearch || includeTvRageSearch || includeTvMazeSearch))
            {
                var ids = "";

                if (includeTvdbSearch)
                {
                    ids += "&tvdbid=" + searchCriteria.Series.TvdbId;
                }

                if (includeImdbSearch)
                {
                    ids += "&imdbid=" + searchCriteria.Series.ImdbId;
                }

                if (includeTvRageSearch)
                {
                    ids += "&rid=" + searchCriteria.Series.TvRageId;
                }

                if (includeTvMazeSearch)
                {
                    ids += "&tvmazeid=" + searchCriteria.Series.TvMazeId;
                }

                chain.Add(GetPagedRequests(MaxPages, categories, "tvsearch", ids + parameters));
            }
            else
            {
                if (includeTvdbSearch)
                {
                    chain.Add(GetPagedRequests(MaxPages,
                        categories,
                        "tvsearch",
                        string.Format("&tvdbid={0}{1}",
                        searchCriteria.Series.TvdbId,
                        parameters)));
                }
                else if (includeImdbSearch)
                {
                    chain.Add(GetPagedRequests(MaxPages,
                        categories,
                        "tvsearch",
                        string.Format("&imdbid={0}{1}",
                        searchCriteria.Series.ImdbId,
                        parameters)));
                }
                else if (includeTvRageSearch)
                {
                    chain.Add(GetPagedRequests(MaxPages,
                        categories,
                        "tvsearch",
                        string.Format("&rid={0}{1}",
                        searchCriteria.Series.TvRageId,
                        parameters)));
                }
                else if (includeTvMazeSearch)
                {
                    chain.Add(GetPagedRequests(MaxPages,
                        categories,
                        "tvsearch",
                        string.Format("&tvmazeid={0}{1}",
                        searchCriteria.Series.TvMazeId,
                        parameters)));
                }
            }
        }

        private void AddTitlePageableRequests(IndexerPageableRequestChain chain, IEnumerable<int> categories, SearchCriteriaBase searchCriteria, string parameters)
        {
            if (SupportsTvTitleSearch)
            {
                foreach (var searchTerm in searchCriteria.SceneTitles)
                {
                    chain.Add(GetPagedRequests(MaxPages,
                        Settings.Categories,
                        "tvsearch",
                        string.Format("&title={0}{1}",
                        Uri.EscapeDataString(searchTerm),
                        parameters)));
                }
            }
            else if (SupportsTvSearch)
            {
                var queryTitles = TvTextSearchEngine == "raw" ? searchCriteria.SceneTitles : searchCriteria.CleanSceneTitles;
                foreach (var queryTitle in queryTitles)
                {
                    chain.Add(GetPagedRequests(MaxPages,
                        Settings.Categories,
                        "tvsearch",
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

            var baseUrl = string.Format("{0}{1}?t={2}&cat={3}&extended=1{4}", Settings.BaseUrl.TrimEnd('/'), Settings.ApiPath.TrimEnd('/'), searchType, categoriesQuery, Settings.AdditionalParameters);

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
            title = title.Replace("+", " ");
            return Uri.EscapeDataString(title);
        }

        // Temporary workaround for NNTMux considering season=0 -> null. '00' should work on existing newznab indexers.
        private static string NewznabifySeasonNumber(int seasonNumber)
        {
            return seasonNumber == 0 ? "00" : seasonNumber.ToString();
        }
    }
}
