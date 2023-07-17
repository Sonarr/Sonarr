using System.Collections.Generic;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.MyAnimeList
{
    public class MyAnimeListRequestGenerator : IImportListRequestGenerator
    {
        public MyAnimeListSettings Settings { get; set; }

        private static readonly Dictionary<MyAnimeListStatus, string> StatusMapping = new Dictionary<MyAnimeListStatus, string>
        {
            { MyAnimeListStatus.Watching, "watching" },
            { MyAnimeListStatus.Completed, "completed" },
            { MyAnimeListStatus.OnHold, "on_hold" },
            { MyAnimeListStatus.Dropped, "dropped" },
            { MyAnimeListStatus.PlanToWatch, "plan_to_watch" },
        };

        public virtual ImportListPageableRequestChain GetListItems()
        {
            var pageableReq = new ImportListPageableRequestChain();

            pageableReq.Add(GetSeriesRequest());

            return pageableReq;
        }

        private IEnumerable<ImportListRequest> GetSeriesRequest()
        {
            var status = (MyAnimeListStatus)Settings.ListStatus;
            var requestBuilder = new HttpRequestBuilder(Settings.BaseUrl.Trim());

            requestBuilder.Resource("users/@me/animelist");
            requestBuilder.AddQueryParam("fields", "list_status");
            requestBuilder.AddQueryParam("limit", "1000");
            requestBuilder.Accept(HttpAccept.Json);

            if (status != MyAnimeListStatus.All && StatusMapping.TryGetValue(status, out var statusName))
            {
                requestBuilder.AddQueryParam("status", statusName);
            }

            var httpReq = new ImportListRequest(requestBuilder.Build());

            httpReq.HttpRequest.Headers.Add("Authorization", $"Bearer {Settings.AccessToken}");

            yield return httpReq;
        }
    }
}
