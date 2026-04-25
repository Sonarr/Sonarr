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
            var requestBuilder = new HttpRequestBuilder(Settings.BaseUrl.Trim());

            var resource = "/shows";

            switch (Settings.TraktListType)
            {
                case (int)TraktPopularListType.Trending:
                    resource += "/trending";
                    break;
                case (int)TraktPopularListType.Popular:
                    resource += "/popular";
                    break;
                case (int)TraktPopularListType.Anticipated:
                    resource += "/anticipated";
                    break;
                case (int)TraktPopularListType.TopWatchedByWeek:
                    resource += "/watched/weekly";
                    break;
                case (int)TraktPopularListType.TopWatchedByMonth:
                    resource += "/watched/monthly";
                    break;
#pragma warning disable CS0612
                case (int)TraktPopularListType.TopWatchedByYear:
#pragma warning restore CS0612
                    resource += "/watched/yearly";
                    break;
                case (int)TraktPopularListType.TopWatchedByAllTime:
                    resource += "/watched/all";
                    break;
                case (int)TraktPopularListType.RecommendedByWeek:
                    resource += "/recommended/weekly";
                    break;
                case (int)TraktPopularListType.RecommendedByMonth:
                    resource += "/recommended/monthly";
                    break;
#pragma warning disable CS0612
                case (int)TraktPopularListType.RecommendedByYear:
#pragma warning restore CS0612
                    resource += "/recommended/yearly";
                    break;
                case (int)TraktPopularListType.RecommendedByAllTime:
                    resource += "/recommended/all";
                    break;
            }

            requestBuilder
                .Resource(resource)
                .Accept(HttpAccept.Json);

            var filterParams = TraktQueryHelper.BuildFilterParameters(Settings.Rating, Settings.Genres, Settings.Years, Settings.Limit, Settings.TraktAdditionalParameters);

            foreach (var param in filterParams)
            {
                requestBuilder.AddQueryParam(param.Key, param.Value);
            }

            requestBuilder
                .SetHeader("trakt-api-version", "2")
                .SetHeader("trakt-api-key", ClientId);

            if (Settings.AccessToken.IsNotNullOrWhiteSpace())
            {
                requestBuilder.SetHeader("Authorization", $"Bearer {Settings.AccessToken}");
            }

            yield return new ImportListRequest(requestBuilder.Build());
        }
    }
}
