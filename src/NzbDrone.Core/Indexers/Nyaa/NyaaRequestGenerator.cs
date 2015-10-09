using System;
using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.Nyaa
{
    public class NyaaRequestGenerator : IIndexerRequestGenerator
    {
        public NyaaSettings Settings { get; set; }

        public int MaxPages { get; set; }
        public int PageSize { get; set; }

        public NyaaRequestGenerator()
        {
            MaxPages = 30;
            PageSize = 100;
        }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetPagedRequests(MaxPages, null));

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SeasonSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(DailyEpisodeSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(AnimeEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            foreach (var queryTitle in searchCriteria.QueryTitles)
            {
                var searchTitle = PrepareQuery(queryTitle);

                pageableRequests.Add(GetPagedRequests(MaxPages,
                    string.Format("&term={0}+{1:0}",
                    searchTitle,
                    searchCriteria.AbsoluteEpisodeNumber)));

                if (searchCriteria.AbsoluteEpisodeNumber < 10)
                {
                    pageableRequests.Add(GetPagedRequests(MaxPages,
                        string.Format("&term={0}+{1:00}",
                        searchTitle,
                        searchCriteria.AbsoluteEpisodeNumber)));
                }
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SpecialEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            foreach (var queryTitle in searchCriteria.EpisodeQueryTitles)
            {
                pageableRequests.Add(GetPagedRequests(MaxPages,
                    string.Format("&term={0}",
                    PrepareQuery(queryTitle))));
            }

            return pageableRequests;
        }

        private IEnumerable<IndexerRequest> GetPagedRequests(int maxPages, string searchParameters)
        {
            var baseUrl = string.Format("{0}/?page=rss{1}", Settings.BaseUrl.TrimEnd('/'), Settings.AdditionalParameters);

            if (PageSize == 0)
            {
                yield return new IndexerRequest(string.Format("{0}{1}", baseUrl, searchParameters), HttpAccept.Rss);
            }
            else
            {
                for (var page = 0; page < maxPages; page++)
                {
                    yield return new IndexerRequest(string.Format("{0}&offset={1}{2}", baseUrl, page + 1, searchParameters), HttpAccept.Rss);
                }
            }
        }

        private string PrepareQuery(string query)
        {
            return query.Replace(' ', '+');
        }
    }
}
