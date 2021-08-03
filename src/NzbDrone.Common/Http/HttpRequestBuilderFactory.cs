namespace NzbDrone.Common.Http
{
    public interface IHttpRequestBuilderFactory
    {
        HttpRequestBuilder Create();
    }

    public class HttpRequestBuilderFactory : IHttpRequestBuilderFactory
    {
        private HttpRequestBuilder _rootBuilder;

        public HttpRequestBuilderFactory(HttpRequestBuilder rootBuilder)
        {
            SetRootBuilder(rootBuilder);
        }

        protected HttpRequestBuilderFactory()
        {
        }

        protected void SetRootBuilder(HttpRequestBuilder rootBuilder)
        {
            _rootBuilder = rootBuilder.Clone();
        }

        public HttpRequestBuilder Create()
        {
            return _rootBuilder.Clone();
        }
    }
}
