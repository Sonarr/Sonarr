using System;
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

        public virtual IList<IEnumerable<IndexerRequest>> GetRecentRequests()
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

            if (capabilities.SupportedTvSearchParameters != null)
            {
                pageableRequests.AddIfNotNull(GetPagedRequests(MaxPages, Settings.Categories.Concat(Settings.AnimeCategories), "tvsearch", ""));
            }

            return pageableRequests;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            if (searchCriteria.Series.TvdbId > 0 && SupportsTvdbSearch)
            {
                pageableRequests.AddIfNotNull(GetPagedRequests(MaxPages, Settings.Categories, "tvsearch",
                    string.Format("&tvdbid={0}&season={1}&ep={2}",
                    searchCriteria.Series.TvdbId,
                    searchCriteria.SeasonNumber,
                    searchCriteria.EpisodeNumber)));
            }
            else if (searchCriteria.Series.TvRageId > 0 && SupportsTvRageSearch)
            {
                pageableRequests.AddIfNotNull(GetPagedRequests(MaxPages, Settings.Categories, "tvsearch",
                    string.Format("&rid={0}&season={1}&ep={2}",
                    searchCriteria.Series.TvRageId,
                    searchCriteria.SeasonNumber,
                    searchCriteria.EpisodeNumber)));
            }
            else if (SupportsTvSearch)
            {
                foreach (var queryTitle in searchCriteria.QueryTitles)
                {
                    pageableRequests.AddIfNotNull(GetPagedRequests(MaxPages, Settings.Categories, "tvsearch",
                        string.Format("&q={0}&season={1}&ep={2}",
                        NewsnabifyTitle(queryTitle),
                        searchCriteria.SeasonNumber,
                        searchCriteria.EpisodeNumber)));
                }
            }

            return pageableRequests;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(SeasonSearchCriteria searchCriteria)
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            if (searchCriteria.Series.TvdbId > 0 && SupportsTvdbSearch)
            {
                pageableRequests.AddIfNotNull(GetPagedRequests(MaxPages, Settings.Categories, "tvsearch",
                    string.Format("&tvdbid={0}&season={1}",
                    searchCriteria.Series.TvdbId,
                    searchCriteria.SeasonNumber)));
            }
            else if (searchCriteria.Series.TvRageId > 0 && SupportsTvRageSearch)
            {
                pageableRequests.AddIfNotNull(GetPagedRequests(MaxPages, Settings.Categories, "tvsearch",
                    string.Format("&rid={0}&season={1}",
                    searchCriteria.Series.TvRageId,
                    searchCriteria.SeasonNumber)));
            }
            else if (SupportsTvSearch)
            {
                foreach (var queryTitle in searchCriteria.QueryTitles)
                {
                    pageableRequests.AddIfNotNull(GetPagedRequests(MaxPages, Settings.Categories, "tvsearch",
                        string.Format("&q={0}&season={1}",
                        NewsnabifyTitle(queryTitle),
                        searchCriteria.SeasonNumber)));
                }
            }

            return pageableRequests;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(DailyEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            if (searchCriteria.Series.TvdbId > 0 && SupportsTvdbSearch)
            {
                pageableRequests.AddIfNotNull(GetPagedRequests(MaxPages, Settings.Categories, "tvsearch",
                    string.Format("&tvdbid={0}&season={1:yyyy}&ep={1:MM}/{1:dd}",
                    searchCriteria.Series.TvdbId,
                    searchCriteria.AirDate)));
            }
            else if (searchCriteria.Series.TvRageId > 0 && SupportsTvRageSearch)
            {
                pageableRequests.AddIfNotNull(GetPagedRequests(MaxPages, Settings.Categories, "tvsearch",
                    string.Format("&rid={0}&season={1:yyyy}&ep={1:MM}/{1:dd}",
                    searchCriteria.Series.TvRageId,
                    searchCriteria.AirDate)));
            }
            else if (SupportsTvSearch)
            {
                foreach (var queryTitle in searchCriteria.QueryTitles)
                {
                    pageableRequests.AddIfNotNull(GetPagedRequests(MaxPages, Settings.Categories, "tvsearch",
                        string.Format("&q={0}&season={1:yyyy}&ep={1:MM}/{1:dd}",
                        NewsnabifyTitle(queryTitle),
                        searchCriteria.AirDate)));
                }
            }

            return pageableRequests;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(AnimeEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            if (SupportsSearch)
            {
                foreach (var queryTitle in searchCriteria.QueryTitles)
                {
                    pageableRequests.AddIfNotNull(GetPagedRequests(MaxPages, Settings.AnimeCategories, "search",
                        string.Format("&q={0}+{1:00}",
                        NewsnabifyTitle(queryTitle),
                        searchCriteria.AbsoluteEpisodeNumber)));
                }
            }

            return pageableRequests;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(SpecialEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            if (SupportsSearch)
            {
                foreach (var queryTitle in searchCriteria.EpisodeQueryTitles)
                {
                    var query = queryTitle.Replace('+', ' ');
                    query = System.Web.HttpUtility.UrlEncode(query);

                    pageableRequests.AddIfNotNull(GetPagedRequests(MaxPages, Settings.Categories.Concat(Settings.AnimeCategories), "search",
                        string.Format("&q={0}",
                        query)));
                }
            }

            return pageableRequests;
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
