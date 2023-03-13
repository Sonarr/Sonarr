using System.Collections.Generic;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Rss
{
    public class RssImportRequestGenerator : IImportListRequestGenerator
    {
        public RssImportBaseSettings Settings { get; set; }

        public virtual ImportListPageableRequestChain GetListItems()
        {
            var pageableRequests = new ImportListPageableRequestChain();

            pageableRequests.Add(GetSeriesRequest());

            return pageableRequests;
        }

        private IEnumerable<ImportListRequest> GetSeriesRequest()
        {
            var request = new ImportListRequest(Settings.Url, HttpAccept.Rss);

            yield return request;
        }
    }
}
