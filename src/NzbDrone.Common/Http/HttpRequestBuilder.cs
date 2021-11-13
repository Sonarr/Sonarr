using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.Http
{
    public class HttpRequestBuilder
    {
        public HttpMethod Method { get; set; }
        public HttpAccept HttpAccept { get; set; }
        public HttpUri BaseUrl { get; private set; }
        public string ResourceUrl { get; set; }
        public List<KeyValuePair<string, string>> QueryParams { get; private set; }
        public List<KeyValuePair<string, string>> SuffixQueryParams { get; private set; }
        public Dictionary<string, string> Segments { get; private set; }
        public HttpHeader Headers { get; private set; }
        public bool SuppressHttpError { get; set; }
        public bool LogHttpError { get; set; }
        public bool UseSimplifiedUserAgent { get; set; }
        public bool AllowAutoRedirect { get; set; }
        public bool ConnectionKeepAlive { get; set; }
        public TimeSpan RateLimit { get; set; }
        public bool LogResponseContent { get; set; }
        public ICredentials NetworkCredential { get; set; }
        public Dictionary<string, string> Cookies { get; private set; }
        public List<HttpFormData> FormData { get; private set; }
        public Action<HttpRequest> PostProcess { get; set; }

        public HttpRequestBuilder(string baseUrl)
        {
            BaseUrl = new HttpUri(baseUrl);
            ResourceUrl = string.Empty;
            Method = HttpMethod.Get;
            QueryParams = new List<KeyValuePair<string, string>>();
            SuffixQueryParams = new List<KeyValuePair<string, string>>();
            Segments = new Dictionary<string, string>();
            Headers = new HttpHeader();
            Cookies = new Dictionary<string, string>();
            FormData = new List<HttpFormData>();
            LogHttpError = true;
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
            clone.FormData = new List<HttpFormData>(clone.FormData);
            return clone;
        }

        protected virtual HttpUri CreateUri()
        {
            var url = BaseUrl.CombinePath(ResourceUrl).AddQueryParams(QueryParams.Concat(SuffixQueryParams));

            if (Segments.Any())
            {
                var fullUri = url.FullUri;

                foreach (var segment in Segments)
                {
                    fullUri = fullUri.Replace(segment.Key, segment.Value);
                }

                url = new HttpUri(fullUri);
            }

            return url;
        }

        protected virtual HttpRequest CreateRequest()
        {
            return new HttpRequest(CreateUri().FullUri, HttpAccept);
        }

        protected virtual void Apply(HttpRequest request)
        {
            request.Method = Method;
            request.SuppressHttpError = SuppressHttpError;
            request.LogHttpError = LogHttpError;
            request.UseSimplifiedUserAgent = UseSimplifiedUserAgent;
            request.AllowAutoRedirect = AllowAutoRedirect;
            request.ConnectionKeepAlive = ConnectionKeepAlive;
            request.RateLimit = RateLimit;
            request.LogResponseContent = LogResponseContent;
            request.Credentials = NetworkCredential;

            foreach (var header in Headers)
            {
                request.Headers.Set(header.Key, header.Value);
            }

            foreach (var cookie in Cookies)
            {
                request.Cookies[cookie.Key] = cookie.Value;
            }

            ApplyFormData(request);
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

        protected virtual void ApplyFormData(HttpRequest request)
        {
            if (FormData.Empty())
            {
                return;
            }

            if (request.ContentData != null)
            {
                throw new ApplicationException("Cannot send HttpRequest Body and FormData simultaneously.");
            }

            var shouldSendAsMultipart = FormData.Any(v => v.ContentType != null || v.FileName != null || v.ContentData.Length > 1024);

            if (shouldSendAsMultipart)
            {
                var boundary = "-----------------------------" + DateTime.Now.Ticks.ToString("x14");
                var partBoundary = string.Format("--{0}\r\n", boundary);
                var endBoundary = string.Format("--{0}--\r\n", boundary);

                var bodyStream = new MemoryStream();
                var summary = new StringBuilder();

                using (var writer = new StreamWriter(bodyStream, new UTF8Encoding(false)))
                {
                    foreach (var formData in FormData)
                    {
                        writer.Write(partBoundary);

                        writer.Write("Content-Disposition: form-data");
                        if (formData.Name.IsNotNullOrWhiteSpace())
                        {
                            writer.Write("; name=\"{0}\"", formData.Name);
                        }

                        if (formData.FileName.IsNotNullOrWhiteSpace())
                        {
                            writer.Write("; filename=\"{0}\"", formData.FileName);
                        }

                        writer.Write("\r\n");

                        if (formData.ContentType.IsNotNullOrWhiteSpace())
                        {
                            writer.Write("Content-Type: {0}\r\n", formData.ContentType);
                        }

                        writer.Write("\r\n");
                        writer.Flush();

                        bodyStream.Write(formData.ContentData, 0, formData.ContentData.Length);
                        writer.Write("\r\n");

                        if (formData.FileName.IsNotNullOrWhiteSpace())
                        {
                            summary.AppendFormat("\r\n{0}={1} ({2} bytes)", formData.Name, formData.FileName, formData.ContentData.Length);
                        }
                        else
                        {
                            summary.AppendFormat("\r\n{0}={1}", formData.Name, Encoding.UTF8.GetString(formData.ContentData));
                        }
                    }

                    writer.Write(endBoundary);
                }

                var body = bodyStream.ToArray();

                // TODO: Scan through body to see if we have a boundary collision?

                request.Headers.ContentType = "multipart/form-data; boundary=" + boundary;
                request.SetContent(body);

                if (request.ContentSummary == null)
                {
                    request.ContentSummary = summary.ToString();
                }
            }
            else
            {
                var parameters = FormData.Select(v => string.Format("{0}={1}", v.Name, Uri.EscapeDataString(Encoding.UTF8.GetString(v.ContentData))));
                var urlencoded = string.Join("&", parameters);
                var body = Encoding.UTF8.GetBytes(urlencoded);

                request.Headers.ContentType = "application/x-www-form-urlencoded";
                request.SetContent(body);

                if (request.ContentSummary == null)
                {
                    request.ContentSummary = urlencoded;
                }
            }
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

        public virtual HttpRequestBuilder KeepAlive(bool keepAlive = true)
        {
            ConnectionKeepAlive = keepAlive;

            return this;
        }

        public virtual HttpRequestBuilder WithRateLimit(double seconds)
        {
            RateLimit = TimeSpan.FromSeconds(seconds);

            return this;
        }

        public virtual HttpRequestBuilder Post()
        {
            Method = HttpMethod.Post;

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

        public virtual HttpRequestBuilder AddPrefixQueryParam(string key, object value, bool replace = false)
        {
            if (replace)
            {
                QueryParams.RemoveAll(v => v.Key == key);
                SuffixQueryParams.RemoveAll(v => v.Key == key);
            }

            QueryParams.Insert(0, new KeyValuePair<string, string>(key, value.ToString()));

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

        public virtual HttpRequestBuilder AddFormParameter(string key, object value)
        {
            if (Method != HttpMethod.Post)
            {
                throw new NotSupportedException("HttpRequest Method must be POST to add FormParameter.");
            }

            FormData.Add(new HttpFormData
            {
                Name = key,
                ContentData = Encoding.UTF8.GetBytes(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture))
            });

            return this;
        }

        public virtual HttpRequestBuilder AddFormUpload(string name, string fileName, byte[] data, string contentType = "application/octet-stream")
        {
            if (Method != HttpMethod.Post)
            {
                throw new NotSupportedException("HttpRequest Method must be POST to add FormUpload.");
            }

            FormData.Add(new HttpFormData
            {
                Name = name,
                FileName = fileName,
                ContentData = data,
                ContentType = contentType
            });

            return this;
        }
    }
}
