using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Trakt.List
{
    public class TraktListRequestGenerator : IImportListRequestGenerator
    {
        public TraktListSettings Settings { get; set; }
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

            requestBuilder
                .Resource("/users/{userName}/lists/{listName}/items/show,season,episode")
                .SetSegment("userName", Settings.Username.Trim())
                .SetSegment("listName", Settings.Listname.ToUrlSlug())
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
