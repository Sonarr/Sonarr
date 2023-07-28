using System.Collections.Generic;
using System.Net.Http;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.AniList.List
{
    public class AniListRequestGenerator : IImportListRequestGenerator
    {
        public AniListSettings Settings { get; set; }
        public string ClientId { get; set; }

        public ImportListPageableRequestChain GetListItems()
        {
            return GetListItems(1);
        }

        public ImportListPageableRequestChain GetListItems(int page)
        {
            var pageableRequests = new ImportListPageableRequestChain();

            pageableRequests.Add(GetRequestList(page));

            return pageableRequests;
        }

        public ImportListRequest GetRequest(int page = 1)
        {
            var request = new ImportListRequest(Settings.BaseUrl.Trim(), HttpAccept.Json);
            request.HttpRequest.Method = HttpMethod.Post;
            request.HttpRequest.Headers.ContentType = "application/json";
            request.HttpRequest.SetContent(AniListAPI.BuildQuery(AniListQuery.QueryList, new
            {
                id = Settings.Username,
                statusType = BuildStatusFilter(),
                page
            }));

            if (Settings.AccessToken.IsNotNullOrWhiteSpace())
            {
                request.HttpRequest.Headers.Add("Authorization", "Bearer " + Settings.AccessToken);
            }

            return request;
        }

        protected IEnumerable<ImportListRequest> GetRequestList(int page = 1)
        {
            yield return GetRequest(page);
        }

        private List<string> BuildStatusFilter()
        {
            var filters = new List<string>();

            if (Settings.ImportCompleted)
            {
                filters.Add(MediaListStatus.Completed);
            }

            if (Settings.ImportCurrent)
            {
                filters.Add(MediaListStatus.Current);
            }

            if (Settings.ImportDropped)
            {
                filters.Add(MediaListStatus.Dropped);
            }

            if (Settings.ImportPaused)
            {
                filters.Add(MediaListStatus.Paused);
            }

            if (Settings.ImportPlanning)
            {
                filters.Add(MediaListStatus.Planning);
            }

            if (Settings.ImportRepeating)
            {
                filters.Add(MediaListStatus.Repeating);
            }

            return filters;
        }
    }
}
