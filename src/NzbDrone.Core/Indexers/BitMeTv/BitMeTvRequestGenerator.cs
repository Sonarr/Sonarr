using System.Collections.Generic;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.BitMeTv
{
    public class BitMeTvRequestGenerator : IIndexerRequestGenerator
    {
        public BitMeTvSettings Settings { get; set; }
        
        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetRssRequests());

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
            return new IndexerPageableRequestChain();
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SpecialEpisodeSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }

        private IEnumerable<IndexerRequest> GetRssRequests()
        {
            var request = new IndexerRequest(string.Format("{0}/rss.php?uid={1}&passkey={2}", Settings.BaseUrl.Trim().TrimEnd('/'), Settings.UserId, Settings.RssPasskey), HttpAccept.Html);

            foreach (var cookie in HttpHeader.ParseCookies(Settings.Cookie))
            {
                request.HttpRequest.Cookies[cookie.Key] = cookie.Value;
            }

            yield return request;
        }
    }
}
