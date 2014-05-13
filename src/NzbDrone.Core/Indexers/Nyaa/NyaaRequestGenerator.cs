using System;
using System.Collections.Generic;
using NzbDrone.Common;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.Nyaa
{
    public class NyaaRequestGenerator : IIndexerRequestGenerator
    {
        public NyaaSettings Settings { get; set; }

        public Int32 MaxPages { get; set; }
        public Int32 PageSize { get; set; }

        public NyaaRequestGenerator()
        {
            MaxPages = 30;
            PageSize = 100;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetRecentRequests()
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            pageableRequests.AddIfNotNull(GetPagedRequests(1, null));

            return pageableRequests;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
        {
            return new List<IEnumerable<IndexerRequest>>();
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(SeasonSearchCriteria searchCriteria)
        {
            return new List<IEnumerable<IndexerRequest>>();
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(DailyEpisodeSearchCriteria searchCriteria)
        {
            return new List<IEnumerable<IndexerRequest>>();
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(AnimeEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            foreach (var queryTitle in searchCriteria.QueryTitles)
            {
                var searchTitle = PrepareQuery(queryTitle);

                pageableRequests.AddIfNotNull(GetPagedRequests(MaxPages,
                    String.Format("&term={0}+{1:0}",
                    searchTitle,
                    searchCriteria.AbsoluteEpisodeNumber)));

                if (searchCriteria.AbsoluteEpisodeNumber < 10)
                {
                    pageableRequests.AddIfNotNull(GetPagedRequests(MaxPages,
                        String.Format("&term={0}+{1:00}",
                        searchTitle,
                        searchCriteria.AbsoluteEpisodeNumber)));
                }
            }

            return pageableRequests;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(SpecialEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            foreach (var queryTitle in searchCriteria.EpisodeQueryTitles)
            {
                pageableRequests.AddIfNotNull(GetPagedRequests(MaxPages,
                    String.Format("&term={0}",
                    PrepareQuery(queryTitle))));
            }

            return pageableRequests;
        }

        private IEnumerable<IndexerRequest> GetPagedRequests(Int32 maxPages, String searchParameters)
        {
            var baseUrl = String.Format("{0}/?page=rss&cats=1_37&filter=1", Settings.BaseUrl.TrimEnd('/'));

            if (PageSize == 0)
            {
                yield return new IndexerRequest(String.Format("{0}{1}", baseUrl, searchParameters), HttpAccept.Rss);
            }
            else
            {
                for (var page = 0; page < maxPages; page++)
                {
                    yield return new IndexerRequest(String.Format("{0}&offset={1}{2}", baseUrl, page + 1, searchParameters), HttpAccept.Rss);
                }
            }
        }

        private String PrepareQuery(String query)
        {
            return query.Replace(' ', '+');
        }
    }
}
