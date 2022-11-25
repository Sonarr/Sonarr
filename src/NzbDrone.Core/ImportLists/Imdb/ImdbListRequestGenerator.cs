using System.Collections.Generic;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Imdb
{
    public class ImdbListRequestGenerator : IImportListRequestGenerator
    {
        public ImdbListSettings Settings { get; set; }

        public virtual ImportListPageableRequestChain GetListItems()
        {
            var pageableRequests = new ImportListPageableRequestChain();
            var httpRequest = new HttpRequest($"https://www.imdb.com/list/{Settings.ListId}/export", new HttpAccept("*/*"));
            var request = new ImportListRequest(httpRequest.Url.ToString(), new HttpAccept(httpRequest.Headers.Accept));

            request.HttpRequest.SuppressHttpError = true;

            pageableRequests.Add(new List<ImportListRequest> { request });

            return pageableRequests;
        }
    }
}
