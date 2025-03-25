using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Trakt.Popular
{
    public class TraktPopularRequestGenerator : IImportListRequestGenerator
    {
        public TraktPopularSettings Settings { get; set; }

        public string ClientId { get; set; }

        public virtual ImportListPageableRequestChain GetListItems()
        {
            var pageableRequests = new ImportListPageableRequestChain();

            pageableRequests.Add(GetSeriesRequest());

            return pageableRequests;
        }

        private IEnumerable<ImportListRequest> GetSeriesRequest()
        {
            var link = Settings.BaseUrl.Trim();

            switch (Settings.TraktListType)
            {
                case (int)TraktPopularListType.Trending:
                    link += "/shows/trending";
                    break;
                case (int)TraktPopularListType.Popular:
                    link += "/shows/popular";
                    break;
                case (int)TraktPopularListType.Anticipated:
                    link += "/shows/anticipated";
                    break;
                case (int)TraktPopularListType.TopWatchedByWeek:
                    link += "/shows/watched/weekly";
                    break;
                case (int)TraktPopularListType.TopWatchedByMonth:
                    link += "/shows/watched/monthly";
                    break;
#pragma warning disable CS0612
                case (int)TraktPopularListType.TopWatchedByYear:
#pragma warning restore CS0612
                    link += "/shows/watched/yearly";
                    break;
                case (int)TraktPopularListType.TopWatchedByAllTime:
                    link += "/shows/watched/all";
                    break;
                case (int)TraktPopularListType.RecommendedByWeek:
                    link += "/shows/recommended/weekly";
                    break;
                case (int)TraktPopularListType.RecommendedByMonth:
                    link += "/shows/recommended/monthly";
                    break;
#pragma warning disable CS0612
                case (int)TraktPopularListType.RecommendedByYear:
#pragma warning restore CS0612
                    link += "/shows/recommended/yearly";
                    break;
                case (int)TraktPopularListType.RecommendedByAllTime:
                    link += "/shows/recommended/all";
                    break;
            }

            var filtersAndLimit = $"?years={Settings.Years}&genres={Settings.Genres?.ToLower()}&ratings={Settings.Rating}&limit={Settings.Limit}{Settings.TraktAdditionalParameters}";
            link += filtersAndLimit;

            var request = new ImportListRequest(link, HttpAccept.Json);

            request.HttpRequest.Headers.Add("trakt-api-version", "2");
            request.HttpRequest.Headers.Add("trakt-api-key", ClientId);

            if (Settings.AccessToken.IsNotNullOrWhiteSpace())
            {
                request.HttpRequest.Headers.Add("Authorization", "Bearer " + Settings.AccessToken);
            }

            yield return request;
        }
    }
}
