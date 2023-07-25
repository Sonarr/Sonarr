using System.Collections.Generic;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.MyAnimeList
{
    public enum MalUserListType
    {
        Watching,
        Completed,
        OnHold,
        Dropped,
        PlanToWatch
    }

    public class MalRequestGenerator : IImportListRequestGenerator
    {
        public MalListSettings Settings { get; set; }

        public virtual ImportListPageableRequestChain GetListItems()
        {
            var pageableReq = new ImportListPageableRequestChain();

            pageableReq.Add(GetSeriesRequest());

            return pageableReq;
        }

        private IEnumerable<ImportListRequest> GetSeriesRequest()
        {
            var url = $"{Settings.BaseUrl.Trim()}/users/@me/animelist?fields=list_status";

            var httpReq = new ImportListRequest(url, HttpAccept.Json);

            httpReq.HttpRequest.Headers.Add("Authorization", $"Bearer {Settings.AccessToken}");

            yield return httpReq;
        }
    }
}
