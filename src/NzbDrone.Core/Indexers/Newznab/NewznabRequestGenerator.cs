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
        public Int32 MaxPages { get; set; }
        public Int32 PageSize { get; set; }
        public NewznabSettings Settings { get; set; }

        public NewznabRequestGenerator()
        {
            MaxPages = 30;
            PageSize = 100;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetRecentRequests()
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            pageableRequests.AddIfNotNull(GetPagedRequests(MaxPages, Settings.Categories.Concat(Settings.AnimeCategories), "tvsearch", ""));

            return pageableRequests;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            if (searchCriteria.Series.TvRageId > 0)
            {
                pageableRequests.AddIfNotNull(GetPagedRequests(MaxPages, Settings.Categories, "tvsearch",
                    String.Format("&rid={0}&season={1}&ep={2}",
                    searchCriteria.Series.TvRageId,
                    searchCriteria.SeasonNumber,
                    searchCriteria.EpisodeNumber)));
            }
            else
            {
                foreach (var queryTitle in searchCriteria.QueryTitles)
                {
                    pageableRequests.AddIfNotNull(GetPagedRequests(MaxPages, Settings.Categories, "tvsearch",
                        String.Format("&q={0}&season={1}&ep={2}",
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

            if (searchCriteria.Series.TvRageId > 0)
            {
                pageableRequests.AddIfNotNull(GetPagedRequests(MaxPages, Settings.Categories, "tvsearch",
                    String.Format("&rid={0}&season={1}",
                    searchCriteria.Series.TvRageId,
                    searchCriteria.SeasonNumber)));
            }
            else
            {
                foreach (var queryTitle in searchCriteria.QueryTitles)
                {
                    pageableRequests.AddIfNotNull(GetPagedRequests(MaxPages, Settings.Categories, "tvsearch",
                        String.Format("&q={0}&season={1}",
                        NewsnabifyTitle(queryTitle),
                        searchCriteria.SeasonNumber)));
                }
            }

            return pageableRequests;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(DailyEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            if (searchCriteria.Series.TvRageId > 0)
            {
                pageableRequests.AddIfNotNull(GetPagedRequests(MaxPages, Settings.Categories, "tvsearch",
                    String.Format("&rid={0}&season={1:yyyy}&ep={1:MM}/{1:dd}",
                    searchCriteria.Series.TvRageId,
                    searchCriteria.AirDate)));
            }
            else
            {
                foreach (var queryTitle in searchCriteria.QueryTitles)
                {
                    pageableRequests.AddIfNotNull(GetPagedRequests(MaxPages, Settings.Categories, "tvsearch",
                        String.Format("&q={0}&season={1:yyyy}&ep={1:MM}/{1:dd}",
                        NewsnabifyTitle(queryTitle),
                        searchCriteria.AirDate)));
                }
            }

            return pageableRequests;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(AnimeEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            foreach (var queryTitle in searchCriteria.QueryTitles)
            {
                pageableRequests.AddIfNotNull(GetPagedRequests(MaxPages, Settings.AnimeCategories, "search",
                    String.Format("&q={0}+{1:00}",
                    NewsnabifyTitle(queryTitle),
                    searchCriteria.AbsoluteEpisodeNumber)));
            }

            return pageableRequests;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(SpecialEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            foreach (var queryTitle in searchCriteria.EpisodeQueryTitles)
            {
                var query = queryTitle.Replace('+', ' ');
                query = System.Web.HttpUtility.UrlEncode(query);

                pageableRequests.AddIfNotNull(GetPagedRequests(MaxPages, Settings.Categories.Concat(Settings.AnimeCategories), "search",
                    String.Format("&q={0}",
                    query)));
            }

            return pageableRequests;
        }

        private IEnumerable<IndexerRequest> GetPagedRequests(Int32 maxPages, IEnumerable<Int32> categories, String searchType, String parameters)
        {
            if (categories.Empty())
            {
                yield break;
            }

            var categoriesQuery = String.Join(",", categories.Distinct());

            var baseUrl = String.Format("{0}/api?t={1}&cat={2}&extended=1{3}", Settings.Url.TrimEnd('/'), searchType, categoriesQuery, Settings.AdditionalParameters);

            if (Settings.ApiKey.IsNotNullOrWhiteSpace())
            {
                baseUrl += "&apikey=" + Settings.ApiKey;
            }

            if (PageSize == 0)
            {
                yield return new IndexerRequest(String.Format("{0}{1}", baseUrl, parameters), HttpAccept.Rss);
            }
            else
            {
                for (var page = 0; page < maxPages; page++)
                {
                    yield return new IndexerRequest(String.Format("{0}&offset={1}&limit={2}{3}", baseUrl, page * PageSize, PageSize, parameters), HttpAccept.Rss);
                }
            }
        }

        private static String NewsnabifyTitle(String title)
        {
            return title.Replace("+", "%20");
        }
    }
}
