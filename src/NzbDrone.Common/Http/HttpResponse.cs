using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Common.Http
{
    public class HttpResponse
    {
        private static readonly Regex RegexSetCookie = new Regex("^(.*?)=(.*?)(?:;|$)", RegexOptions.Compiled);

        public HttpResponse(HttpRequest request, HttpHeader headers, byte[] binaryData, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            Request = request;
            Headers = headers;
            ResponseData = binaryData;
            StatusCode = statusCode;
        }

        public HttpResponse(HttpRequest request, HttpHeader headers, string content, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            Request = request;
            Headers = headers;
            ResponseData = Headers.GetEncodingFromContentType().GetBytes(content);
            _content = content;
            StatusCode = statusCode;
        }

        public HttpRequest Request { get; private set; }
        public HttpHeader Headers { get; private set; }
        public HttpStatusCode StatusCode { get; private set; }
        public byte[] ResponseData { get; private set; }

        private string _content;

        public string Content
        {
            get
            {
                if (_content == null)
                {
                    _content = Headers.GetEncodingFromContentType().GetString(ResponseData);
                }

                return _content;
            }
        }

        public bool HasHttpError => (int)StatusCode >= 400;

        public bool HasHttpRedirect => StatusCode == HttpStatusCode.Moved ||
                                       StatusCode == HttpStatusCode.MovedPermanently ||
                                       StatusCode == HttpStatusCode.Found ||
                                       StatusCode == HttpStatusCode.TemporaryRedirect ||
                                       StatusCode == HttpStatusCode.RedirectMethod ||
                                       StatusCode == HttpStatusCode.SeeOther;

        public string[] GetCookieHeaders()
        {
            return Headers.GetValues("Set-Cookie") ?? new string[0];
        }

        public Dictionary<string, string> GetCookies()
        {
            var result = new Dictionary<string, string>();

            var setCookieHeaders = GetCookieHeaders();
            foreach (var cookie in setCookieHeaders)
            {
                var match = RegexSetCookie.Match(cookie);
                if (match.Success)
                {
                    result[match.Groups[1].Value] = match.Groups[2].Value;
                }
            }

            return result;
        }

        public override string ToString()
        {
            var result = string.Format("Res: [{0}] {1}: {2}.{3} ({4} bytes)", Request.Method, Request.Url, (int)StatusCode, StatusCode, ResponseData?.Length ?? 0);

            if (HasHttpError && Headers.ContentType.IsNotNullOrWhiteSpace() && !Headers.ContentType.Equals("text/html", StringComparison.InvariantCultureIgnoreCase))
            {
                result += Environment.NewLine + Content;
            }

            return result;
        }
    }

    public class HttpResponse<T> : HttpResponse
        where T : new()
    {
        public HttpResponse(HttpResponse response)
            : base(response.Request, response.Headers, response.ResponseData, response.StatusCode)
        {
            Resource = Json.Deserialize<T>(response.Content);
        }

        public T Resource { get; private set; }
    }
}
