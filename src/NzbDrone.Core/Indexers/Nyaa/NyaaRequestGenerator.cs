using System.Collections.Generic;
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
            var pageableRequests = new IndexerPageableRequestChain();

            if (Settings.AnimeStandardFormatSearch && searchCriteria.SeasonNumber > 0)
            {
                foreach (var queryTitle in searchCriteria.SceneTitles)
                {
                    var searchTitle = PrepareQuery(queryTitle);

                    pageableRequests.Add(GetPagedRequests(MaxPages, $"{searchTitle}+s{searchCriteria.SeasonNumber:00}"));
                }
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(DailyEpisodeSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(DailySeasonSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(AnimeEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            foreach (var queryTitle in searchCriteria.SceneTitles)
            {
                var searchTitle = PrepareQuery(queryTitle);

                if (searchCriteria.AbsoluteEpisodeNumber > 0)
                {
                    pageableRequests.Add(GetPagedRequests(MaxPages, $"{searchTitle}+{searchCriteria.AbsoluteEpisodeNumber:0}"));

                    if (searchCriteria.AbsoluteEpisodeNumber < 10)
                    {
                        pageableRequests.Add(GetPagedRequests(MaxPages, $"{searchTitle}+{searchCriteria.AbsoluteEpisodeNumber:00}"));
                    }
                }

                if (Settings.AnimeStandardFormatSearch && searchCriteria.SeasonNumber > 0 && searchCriteria.EpisodeNumber > 0)
                {
                    pageableRequests.Add(GetPagedRequests(MaxPages, $"{searchTitle}+s{searchCriteria.SeasonNumber:00}e{searchCriteria.EpisodeNumber:00}"));
                }
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SpecialEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            foreach (var queryTitle in searchCriteria.EpisodeQueryTitles)
            {
                pageableRequests.Add(GetPagedRequests(MaxPages, PrepareQuery(queryTitle)));
            }

            return pageableRequests;
        }

        private IEnumerable<IndexerRequest> GetPagedRequests(int maxPages, string term)
        {
            var baseUrl = string.Format("{0}/?page=rss{1}", Settings.BaseUrl.TrimEnd('/'), Settings.AdditionalParameters);

            if (term != null)
            {
                baseUrl += "&term=" + term;
            }

            if (PageSize == 0)
            {
                yield return new IndexerRequest(baseUrl, HttpAccept.Rss);
            }
            else
            {
                yield return new IndexerRequest(baseUrl, HttpAccept.Rss);

                for (var page = 1; page < maxPages; page++)
                {
                    yield return new IndexerRequest($"{baseUrl}&offset={page + 1}", HttpAccept.Rss);
                }
            }
        }

        private string PrepareQuery(string query)
        {
            return query.Replace(' ', '+');
        }
    }
}
