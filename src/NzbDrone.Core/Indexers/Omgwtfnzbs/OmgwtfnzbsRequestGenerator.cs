using System;
using System.Collections.Generic;
using System.Text;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.Omgwtfnzbs
{
    public class OmgwtfnzbsRequestGenerator : IIndexerRequestGenerator
    {
        public string BaseUrl { get; set; }
        public OmgwtfnzbsSettings Settings { get; set; }

        public OmgwtfnzbsRequestGenerator()
        {
            BaseUrl = "https://rss.omgwtfnzbs.org/rss-download.php";
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetRecentRequests()
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            pageableRequests.AddIfNotNull(GetPagedRequests(null));

            return pageableRequests;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            foreach (var queryTitle in searchCriteria.QueryTitles)
            {
                pageableRequests.AddIfNotNull(GetPagedRequests(string.Format("{0}+S{1:00}E{2:00}",
                    queryTitle,
                    searchCriteria.SeasonNumber,
                    searchCriteria.EpisodeNumber)));
            }

            return pageableRequests;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(SeasonSearchCriteria searchCriteria)
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            foreach (var queryTitle in searchCriteria.QueryTitles)
            {
                pageableRequests.AddIfNotNull(GetPagedRequests(string.Format("{0}+S{1:00}",
                    queryTitle,
                    searchCriteria.SeasonNumber)));
            }

            return pageableRequests;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(DailyEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            foreach (var queryTitle in searchCriteria.QueryTitles)
            {
                pageableRequests.AddIfNotNull(GetPagedRequests(string.Format("{0}+{1:yyyy MM dd}",
                    queryTitle,
                    searchCriteria.AirDate)));
            }

            return pageableRequests;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(AnimeEpisodeSearchCriteria searchCriteria)
        {
            return new List<IEnumerable<IndexerRequest>>();
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(SpecialEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            foreach (var queryTitle in searchCriteria.EpisodeQueryTitles)
            {
                var query = queryTitle.Replace('+', ' ');
                query = System.Web.HttpUtility.UrlEncode(query);

                pageableRequests.AddIfNotNull(GetPagedRequests(query));
            }

            return pageableRequests;
        }

        private IEnumerable<IndexerRequest> GetPagedRequests(string query)
        {
            var url = new StringBuilder();
            url.AppendFormat("{0}?catid=19,20&user={1}&api={2}&eng=1&delay={3}", BaseUrl, Settings.Username, Settings.ApiKey, Settings.Delay);

            if (query.IsNotNullOrWhiteSpace())
            {
                url = url.Replace("rss-download.php", "rss-search.php");
                url.AppendFormat("&search={0}", query);
            }

            yield return new IndexerRequest(url.ToString(), HttpAccept.Rss);
        }
    }
}
