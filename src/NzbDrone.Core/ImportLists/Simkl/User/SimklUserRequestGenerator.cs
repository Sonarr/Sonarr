using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Simkl.User
{
    public class SimklUserRequestGenerator : IImportListRequestGenerator
    {
        public SimklUserSettings Settings { get; set; }

        public string ClientId { get; set; }

        public SimklUserRequestGenerator()
        {
        }

        public virtual ImportListPageableRequestChain GetListItems()
        {
            var pageableRequests = new ImportListPageableRequestChain();

            pageableRequests.Add(GetSeriesRequest());

            return pageableRequests;
        }

        private IEnumerable<ImportListRequest> GetSeriesRequest()
        {
            var link = Settings.BaseUrl.Trim();

            switch (Settings.ListType)
            {
                case (int)SimklUserListType.Watching:
                    link += $"/sync/all-items/shows/watching";
                    break;
                case (int)SimklUserListType.PlanToWatch:
                    link += $"/sync/all-items/shows/plantowatch";
                    break;
                case (int)SimklUserListType.Hold:
                    link += $"/sync/all-items/shows/hold";
                    break;
                case (int)SimklUserListType.Completed:
                    link += $"/sync/all-items/shows/completed";
                    break;
                case (int)SimklUserListType.Dropped:
                    link += $"/sync/all-items/shows/dropped";
                    break;
            }

            var request = new ImportListRequest($"{link}", HttpAccept.Json);

            request.HttpRequest.Headers.Add("simkl-api-key", ClientId);

            if (Settings.AccessToken.IsNotNullOrWhiteSpace())
            {
                request.HttpRequest.Headers.Add("Authorization", "Bearer " + Settings.AccessToken);
            }

            yield return request;
        }
    }
}
