using Workarr.Http;

namespace Workarr.ImportLists
{
    public class ImportListRequest
    {
        public HttpRequest HttpRequest { get; private set; }

        public ImportListRequest(string url, HttpAccept httpAccept)
        {
            HttpRequest = new HttpRequest(url, httpAccept);
        }

        public ImportListRequest(HttpRequest httpRequest)
        {
            HttpRequest = httpRequest;
        }

        public HttpUri Url => HttpRequest.Url;
    }
}
