using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.Http
{
    public class HttpRequestBuilder
    {
        public HttpMethod Method { get; set; }
        public HttpAccept HttpAccept { get; set; }
        public Uri BaseUrl { get; private set; }
        public string ResourceUrl { get; set; }
        public List<KeyValuePair<string, string>> QueryParams { get; private set; }
        public List<KeyValuePair<string, string>> SuffixQueryParams { get; private set; }
        public Dictionary<string, string> Segments { get; private set; }
        public HttpHeader Headers { get; private set; }
        public bool SuppressHttpError { get; set; }
        public bool AllowAutoRedirect { get; set; }
        public NetworkCredential NetworkCredential { get; set; }
        public Dictionary<string, string> Cookies { get; private set; }

        public Action<HttpRequest> PostProcess { get; set; }

        public HttpRequestBuilder(string baseUrl)
        {
            BaseUrl = new Uri(baseUrl);
            ResourceUrl = string.Empty;
            Method = HttpMethod.GET;
            QueryParams = new List<KeyValuePair<string, string>>();
            SuffixQueryParams = new List<KeyValuePair<string, string>>();
            Segments = new Dictionary<string, string>();
            Headers = new HttpHeader();
            Cookies = new Dictionary<string, string>();
        }

        public HttpRequestBuilder(bool useHttps, string host, int port, string urlBase = null)
            : this(BuildBaseUrl(useHttps, host, port, urlBase))
        {

        }

        public static string BuildBaseUrl(bool useHttps, string host, int port, string urlBase = null)
        {
            var protocol = useHttps ? "https" : "http";

            if (urlBase.IsNotNullOrWhiteSpace() && !urlBase.StartsWith("/"))
            {
                urlBase = "/" + urlBase;
            }

            return string.Format("{0}://{1}:{2}{3}", protocol, host, port, urlBase);
        }

        public virtual HttpRequestBuilder Clone()
        {
            var clone = MemberwiseClone() as HttpRequestBuilder;
            clone.QueryParams = new List<KeyValuePair<string, string>>(clone.QueryParams);
            clone.SuffixQueryParams = new List<KeyValuePair<string, string>>(clone.SuffixQueryParams);
            clone.Segments = new Dictionary<string, string>(clone.Segments);
            clone.Headers = new HttpHeader(clone.Headers);
            clone.Cookies = new Dictionary<string, string>(clone.Cookies);
            return clone;
        }

        protected virtual Uri CreateUri()
        {
            var builder = new UriBuilder(new Uri(BaseUrl, ResourceUrl));
            
            foreach (var queryParam in QueryParams.Concat(SuffixQueryParams))
            {
                builder.SetQueryParam(queryParam.Key, queryParam.Value);
            }

            if (Segments.Any())
            {
                var url = builder.Uri.ToString();

                foreach (var segment in Segments)
                {
                    url = url.Replace(segment.Key, segment.Value);
                }

                builder = new UriBuilder(url);
            }

            return builder.Uri;
        }

        protected virtual HttpRequest CreateRequest()
        {
            return new HttpRequest(CreateUri().ToString(), HttpAccept);
        }

        protected virtual void Apply(HttpRequest request)
        {
            request.Method = Method;
            request.SuppressHttpError = SuppressHttpError;
            request.AllowAutoRedirect = AllowAutoRedirect;
            request.NetworkCredential = NetworkCredential;

            foreach (var header in Headers)
            {
                request.Headers.Set(header.Key, header.Value);
            }

            foreach (var cookie in Cookies)
            {
                request.Cookies[cookie.Key] = cookie.Value;
            }
        }

        public virtual HttpRequest Build()
        {
            var request = CreateRequest();

            Apply(request);

            if (PostProcess != null)
            {
                PostProcess(request);
            }

            return request;
        }

        public IHttpRequestBuilderFactory CreateFactory()
        {
            return new HttpRequestBuilderFactory(this);
        }

        public virtual HttpRequestBuilder Resource(string resourceUrl)
        {
            if (!ResourceUrl.IsNotNullOrWhiteSpace() || resourceUrl.StartsWith("/"))
            {
                ResourceUrl = resourceUrl.TrimStart('/');
            }
            else
            {
                ResourceUrl = string.Format("{0}/{1}", ResourceUrl.TrimEnd('/'), resourceUrl);
            }

            return this;
        }

        public virtual HttpRequestBuilder Post()
        {
            Method = HttpMethod.POST;

            return this;
        }

        public virtual HttpRequestBuilder Accept(HttpAccept accept)
        {
            HttpAccept = accept;

            return this;
        }

        public virtual HttpRequestBuilder SetHeader(string name, string value)
        {
            Headers.Set(name, value);

            return this;
        }

        public virtual HttpRequestBuilder AddQueryParam(string key, object value, bool replace = false)
        {
            if (replace)
            {
                QueryParams.RemoveAll(v => v.Key == key);
                SuffixQueryParams.RemoveAll(v => v.Key == key);
            }

            QueryParams.Add(key, value.ToString());

            return this;
        }

        public virtual HttpRequestBuilder AddSuffixQueryParam(string key, object value, bool replace = false)
        {
            if (replace)
            {
                QueryParams.RemoveAll(v => v.Key == key);
                SuffixQueryParams.RemoveAll(v => v.Key == key);
            }

            SuffixQueryParams.Add(new KeyValuePair<string, string>(key, value.ToString()));

            return this;
        }

        public virtual HttpRequestBuilder SetSegment(string segment, string value, bool dontCheck = false)
        {
            var key = string.Concat("{", segment, "}");

            if (!dontCheck && !CreateUri().ToString().Contains(key))
            {
                throw new InvalidOperationException(string.Format("Segment {0} is not defined in Uri", segment));
            }

            Segments[key] = value;

            return this;
        }

        public virtual HttpRequestBuilder SetCookies(IEnumerable<KeyValuePair<string, string>> cookies)
        {
            foreach (var cookie in cookies)
            {
                Cookies[cookie.Key] = cookie.Value;
            }

            return this;
        }

        public virtual HttpRequestBuilder SetCookie(string key, string value)
        {
            Cookies[key] = value;

            return this;
        }
    }
}