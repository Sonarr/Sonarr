using System.Collections.Generic;
using System.Text;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Indexers.Torrentleech
{
    public class TorrentleechRequestGenerator : IIndexerRequestGenerator
    {
        public TorrentleechSettings Settings { get; set; }
        public IHttpClient HttpClient { get; set; }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetRssRequests(null));

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetSearchRequest(searchCriteria.Series, searchCriteria.SeasonNumber, searchCriteria.EpisodeNumber));

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SeasonSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetSearchRequest(searchCriteria.Series, searchCriteria.SeasonNumber));

            return pageableRequests;
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

        private IEnumerable<IndexerRequest> GetSearchRequest(Series series, int seasonNumber, int? episodeNumber = null)
        {
            var requestList = new List<IndexerRequest>();

            var loginRequest = new HttpRequest(Settings.LoginUrl, HttpAccept.Html)
            {
                Method = HttpMethod.POST,
                StoreResponseCookie = true
            };
            loginRequest.Headers.ContentType = "application/x-www-form-urlencoded";
            loginRequest.SetContent($"username={Settings.Username}&password={Settings.Password}");

            //var loginIndexerRequest = new IndexerRequest(loginRequest);

            //requestList.Add(loginIndexerRequest);

            HttpClient.Execute(loginRequest);

            var requestUrl = new StringBuilder();
            requestUrl.Append(Settings.RequestUrl);
            requestUrl.Append($"{SearchCriteriaBase.GetQueryTitle(series.Title)} S{seasonNumber:00}");

            requestUrl.Append(episodeNumber.HasValue ? $"E{episodeNumber:00}" : "/categories/27");

            requestList.Add(new IndexerRequest(requestUrl.ToString(), HttpAccept.Html));

            return requestList;
        }

        private IEnumerable<IndexerRequest> GetRssRequests(string searchParameters)
        {
            yield return new IndexerRequest(string.Format("https://rss.torrentleech.org/{1}{2}", Settings.ApiKey, searchParameters), HttpAccept.Rss);
        }
    }
}



