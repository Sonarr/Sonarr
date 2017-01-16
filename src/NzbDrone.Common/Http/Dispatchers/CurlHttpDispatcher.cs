using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using CurlSharp;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http.Proxy;

namespace NzbDrone.Common.Http.Dispatchers
{
    public class CurlHttpDispatcher : IHttpDispatcher
    {
        private static readonly Regex ExpiryDate = new Regex(@"(expires=)([^;]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly IHttpProxySettingsProvider _proxySettingsProvider;
        private readonly IUserAgentBuilder _userAgentBuilder;
        private readonly Logger _logger;

        private const string _caBundleFileName = "curl-ca-bundle.crt";
        private static readonly string _caBundleFilePath;

        static CurlHttpDispatcher()
        {
            if (Assembly.GetExecutingAssembly().Location.IsNotNullOrWhiteSpace())
            {
                _caBundleFilePath = Path.Combine(Assembly.GetExecutingAssembly().Location, "..", _caBundleFileName);
            }
            else
            {
                _caBundleFilePath = _caBundleFileName;
            }
        }
        
        public CurlHttpDispatcher(IHttpProxySettingsProvider proxySettingsProvider, IUserAgentBuilder userAgentBuilder, Logger logger)
        {
            _proxySettingsProvider = proxySettingsProvider;
            _userAgentBuilder = userAgentBuilder;
            _logger = logger;
        }

        public bool CheckAvailability()
        {
            try
            {
                return CurlGlobalHandle.Instance.Initialize();
            }
            catch (Exception ex)
            {
                _logger.Trace(ex, "Initializing curl failed");
                return false;
            }
        }

        public HttpResponse GetResponse(HttpRequest request, CookieContainer cookies)
        {
            if (!CheckAvailability())
            {
                throw new ApplicationException("Curl failed to initialize.");
            }

            lock (CurlGlobalHandle.Instance)
            {
                Stream responseStream = new MemoryStream();
                Stream headerStream = new MemoryStream();

                using (var curlEasy = new CurlEasy())
                {
                    curlEasy.AutoReferer = false;
                    curlEasy.WriteFunction = (b, s, n, o) =>
                    {
                        responseStream.Write(b, 0, s * n);
                        return s * n;
                    };
                    curlEasy.HeaderFunction = (b, s, n, o) =>
                    {
                        headerStream.Write(b, 0, s * n);
                        return s * n;
                    };

                    AddProxy(curlEasy, request);

                    curlEasy.Url = request.Url.FullUri;

                    switch (request.Method)
                    {
                        case HttpMethod.GET:
                            curlEasy.HttpGet = true;
                            break;

                        case HttpMethod.POST:
                            curlEasy.Post = true;
                            break;

                        case HttpMethod.PUT:
                            curlEasy.Put = true;
                            break;

                        default:
                            throw new NotSupportedException($"HttpCurl method {request.Method} not supported");
                    }
                    curlEasy.UserAgent = _userAgentBuilder.GetUserAgent(request.UseSimplifiedUserAgent);
                    curlEasy.FollowLocation = request.AllowAutoRedirect;

                    if (request.RequestTimeout != TimeSpan.Zero)
                    {
                        curlEasy.Timeout = (int)Math.Ceiling(request.RequestTimeout.TotalSeconds);
                    }

                    if (OsInfo.IsWindows)
                    {
                        curlEasy.CaInfo = _caBundleFilePath;
                    }

                    if (cookies != null)
                    {
                        curlEasy.Cookie = cookies.GetCookieHeader((Uri)request.Url);
                    }

                    if (request.ContentData != null)
                    {
                        curlEasy.PostFieldSize = request.ContentData.Length;
                        curlEasy.SetOpt(CurlOption.CopyPostFields, new string(Array.ConvertAll(request.ContentData, v => (char)v)));
                    }

                    // Yes, we have to keep a ref to the object to prevent corrupting the unmanaged state
                    using (var httpRequestHeaders = SerializeHeaders(request))
                    {
                        curlEasy.HttpHeader = httpRequestHeaders;

                        var result = curlEasy.Perform();

                        if (result != CurlCode.Ok)
                        {
                            switch (result)
                            {
                                case CurlCode.SslCaCert:
                                case (CurlCode)77:
                                    throw new WebException(string.Format("Curl Error {0} for Url {1}, issues with your operating system SSL Root Certificate Bundle (ca-bundle).", result, curlEasy.Url));
                                default:
                                    throw new WebException(string.Format("Curl Error {0} for Url {1}", result, curlEasy.Url));

                            }
                        }
                    }

                    var webHeaderCollection = ProcessHeaderStream(request, cookies, headerStream);
                    var responseData = ProcessResponseStream(request, responseStream, webHeaderCollection);

                    var httpHeader = new HttpHeader(webHeaderCollection);

                    return new HttpResponse(request, httpHeader, responseData, (HttpStatusCode)curlEasy.ResponseCode);
                }
            }
        }

        private void AddProxy(CurlEasy curlEasy, HttpRequest request)
        {
            var proxySettings = _proxySettingsProvider.GetProxySettings(request);
            if (proxySettings != null)

            {
                switch (proxySettings.Type)
                {
                    case ProxyType.Http:
                        curlEasy.SetOpt(CurlOption.ProxyType, CurlProxyType.Http);
                        curlEasy.SetOpt(CurlOption.ProxyAuth, CurlHttpAuth.Basic);
                        curlEasy.SetOpt(CurlOption.ProxyUserPwd, proxySettings.Username + ":" + proxySettings.Password.ToString());
                        break;
                    case ProxyType.Socks4:
                        curlEasy.SetOpt(CurlOption.ProxyType, CurlProxyType.Socks4);
                        curlEasy.SetOpt(CurlOption.ProxyUsername, proxySettings.Username);
                        curlEasy.SetOpt(CurlOption.ProxyPassword, proxySettings.Password);
                        break;
                    case ProxyType.Socks5:
                        curlEasy.SetOpt(CurlOption.ProxyType, CurlProxyType.Socks5);
                        curlEasy.SetOpt(CurlOption.ProxyUsername, proxySettings.Username);
                        curlEasy.SetOpt(CurlOption.ProxyPassword, proxySettings.Password);
                        break;
                }
                curlEasy.SetOpt(CurlOption.Proxy, proxySettings.Host + ":" + proxySettings.Port.ToString());
            }
        }

        private CurlSlist SerializeHeaders(HttpRequest request)
        {
            if (!request.Headers.ContainsKey("Accept-Encoding"))
            {
                request.Headers.Add("Accept-Encoding", "gzip");
            }

            if (request.Headers.ContentType == null)
            {
                request.Headers.ContentType = string.Empty;
            }

            var curlHeaders = new CurlSlist();
            foreach (var header in request.Headers)
            {
                curlHeaders.Append(header.Key + ": " + header.Value.ToString());
            }

            return curlHeaders;
        }

        private WebHeaderCollection ProcessHeaderStream(HttpRequest request, CookieContainer cookies, Stream headerStream)
        {
            headerStream.Position = 0;
            var headerData = headerStream.ToBytes();
            var headerString = Encoding.ASCII.GetString(headerData);

            var webHeaderCollection = new WebHeaderCollection();

            // following a redirect we could have two sets of headers, so only process the last one
            foreach (var header in headerString.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Reverse())
            {
                if (!header.Contains(":")) break;
                webHeaderCollection.Add(header);
            }

            var setCookie = webHeaderCollection.Get("Set-Cookie");
            if (setCookie != null && setCookie.Length > 0 && cookies != null)
            {
                try
                {
                    cookies.SetCookies((Uri)request.Url, FixSetCookieHeader(setCookie));
                }
                catch (CookieException ex)
                {
                    _logger.Debug("Rejected cookie {0}: {1}", ex.InnerException.Message, setCookie);
                }
            }

            return webHeaderCollection;
        }

        private string FixSetCookieHeader(string setCookie)
        {
            // fix up the date if it was malformed
            var setCookieClean = ExpiryDate.Replace(setCookie, delegate(Match match)
            {
                string shortFormat = "ddd, dd-MMM-yy HH:mm:ss";
                string longFormat = "ddd, dd-MMM-yyyy HH:mm:ss";
                DateTime dt;
                if (DateTime.TryParseExact(match.Groups[2].Value, longFormat, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out dt) ||
                    DateTime.TryParseExact(match.Groups[2].Value, shortFormat, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out dt) ||
                    DateTime.TryParse(match.Groups[2].Value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out dt))
                    return match.Groups[1].Value + dt.ToUniversalTime().ToString(longFormat, CultureInfo.InvariantCulture) + " GMT";
                else
                    return match.Value;
            });
            return setCookieClean;
        }

        private byte[] ProcessResponseStream(HttpRequest request, Stream responseStream, WebHeaderCollection webHeaderCollection)
        {
            responseStream.Position = 0;

            if (responseStream.Length != 0)
            {
                var encoding = webHeaderCollection["Content-Encoding"];
                if (encoding != null)
                {
                    if (encoding.IndexOf("gzip") != -1)
                    {
                        responseStream = new GZipStream(responseStream, CompressionMode.Decompress);

                        webHeaderCollection.Remove("Content-Encoding");
                    }
                    else if (encoding.IndexOf("deflate") != -1)
                    {
                        responseStream = new DeflateStream(responseStream, CompressionMode.Decompress);

                        webHeaderCollection.Remove("Content-Encoding");
                    }
                }
            }

            return responseStream.ToBytes();

        }
    }

    internal sealed class CurlGlobalHandle : SafeHandle
    {
        public static readonly CurlGlobalHandle Instance = new CurlGlobalHandle();

        private bool _initialized;
        private bool _available;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        private CurlGlobalHandle()
            : base(IntPtr.Zero, true)
        {

        }

        public bool Initialize()
        {
            lock (CurlGlobalHandle.Instance)
            {
                if (_initialized)
                    return _available;

                _initialized = true;
                _available = Curl.GlobalInit(CurlInitFlag.All) == CurlCode.Ok;

                return _available;
            }
        }

        protected override bool ReleaseHandle()
        {
            if (_initialized && _available)
            {
                Curl.GlobalCleanup();
                _available = false;
            }
            return true;
        }

        public override bool IsInvalid => !_initialized || !_available;
    }
}
