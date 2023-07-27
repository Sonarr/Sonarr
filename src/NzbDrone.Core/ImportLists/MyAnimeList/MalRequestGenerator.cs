using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.MyAnimeList
{
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
            var selectedListStatus = JsonConvert.SerializeObject(Settings.ListStatus, new StringEnumConverter()).Replace("\"", "");
            var url = $"{Settings.BaseUrl.Trim()}/users/@me/animelist?fields=list_status&limit=1000&status={selectedListStatus}";

            var httpReq = new ImportListRequest(url, HttpAccept.Json);

            httpReq.HttpRequest.Headers.Add("Authorization", $"Bearer {Settings.AccessToken}");

            yield return httpReq;
        }
    }
}
