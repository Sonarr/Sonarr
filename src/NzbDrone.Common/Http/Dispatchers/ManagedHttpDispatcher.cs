using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http.Proxy;

namespace NzbDrone.Common.Http.Dispatchers
{
    public class ManagedHttpDispatcher : IHttpDispatcher
    {
        private const string NO_PROXY_KEY = "no-proxy";

        private const int connection_establish_timeout = 2000;
        private static bool useIPv6 = Socket.OSSupportsIPv6;
        private static bool hasResolvedIPv6Availability;

        private readonly IHttpProxySettingsProvider _proxySettingsProvider;
        private readonly ICreateManagedWebProxy _createManagedWebProxy;
        private readonly ICertificateValidationService _certificateValidationService;
        private readonly IUserAgentBuilder _userAgentBuilder;
        private readonly ICached<System.Net.Http.HttpClient> _httpClientCache;
        private readonly ICached<CredentialCache> _credentialCache;

        public ManagedHttpDispatcher(IHttpProxySettingsProvider proxySettingsProvider,
            ICreateManagedWebProxy createManagedWebProxy,
            ICertificateValidationService certificateValidationService,
            IUserAgentBuilder userAgentBuilder,
            ICacheManager cacheManager)
        {
            _proxySettingsProvider = proxySettingsProvider;
            _createManagedWebProxy = createManagedWebProxy;
            _certificateValidationService = certificateValidationService;
            _userAgentBuilder = userAgentBuilder;

            _httpClientCache = cacheManager.GetCache<System.Net.Http.HttpClient>(typeof(ManagedHttpDispatcher));
            _credentialCache = cacheManager.GetCache<CredentialCache>(typeof(ManagedHttpDispatcher), "credentialcache");
        }

        public async Task<HttpResponse> GetResponseAsync(HttpRequest request, CookieContainer cookies)
        {
            var requestMessage = new HttpRequestMessage(request.Method, (Uri)request.Url)
            {
                Version = HttpVersion.Version20,
                VersionPolicy = HttpVersionPolicy.RequestVersionOrLower
            };
            requestMessage.Headers.UserAgent.ParseAdd(_userAgentBuilder.GetUserAgent(request.UseSimplifiedUserAgent));
            requestMessage.Headers.ConnectionClose = !request.ConnectionKeepAlive;

            var cookieHeader = cookies.GetCookieHeader((Uri)request.Url);
            if (cookieHeader.IsNotNullOrWhiteSpace())
            {
                requestMessage.Headers.Add("Cookie", cookieHeader);
            }

            using var cts = new CancellationTokenSource();
            if (request.RequestTimeout != TimeSpan.Zero)
            {
                cts.CancelAfter(request.RequestTimeout);
            }
            else
            {
                // The default for System.Net.Http.HttpClient
                cts.CancelAfter(TimeSpan.FromSeconds(100));
            }

            if (request.Credentials != null)
            {
                if (request.Credentials is BasicNetworkCredential bc)
                {
                    // Manually set header to avoid initial challenge response
                    var authInfo = bc.UserName + ":" + bc.Password;
                    authInfo = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(authInfo));
                    requestMessage.Headers.Add("Authorization", "Basic " + authInfo);
                }
                else if (request.Credentials is NetworkCredential nc)
                {
                    var creds = GetCredentialCache();
                    foreach (var authtype in new[] { "Basic", "Digest" })
                    {
                        creds.Remove((Uri)request.Url, authtype);
                        creds.Add((Uri)request.Url, authtype, nc);
                    }
                }
            }

            if (request.ContentData != null)
            {
                requestMessage.Content = new ByteArrayContent(request.ContentData);
            }

            if (request.Headers != null)
            {
                AddRequestHeaders(requestMessage, request.Headers);
            }

            var httpClient = GetClient(request.Url);

            try
            {
                using var responseMessage = await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cts.Token);
                {
                    byte[] data = null;

                    try
                    {
                        if (request.ResponseStream != null && responseMessage.StatusCode == HttpStatusCode.OK)
                        {
                            await responseMessage.Content.CopyToAsync(request.ResponseStream, null, cts.Token);
                        }
                        else
                        {
                            data = await responseMessage.Content.ReadAsByteArrayAsync(cts.Token);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new WebException("Failed to read complete http response", ex, WebExceptionStatus.ReceiveFailure, null);
                    }

                    var headers = responseMessage.Headers.ToNameValueCollection();

                    headers.Add(responseMessage.Content.Headers.ToNameValueCollection());

                    return new HttpResponse(request, new HttpHeader(headers), data, responseMessage.StatusCode, responseMessage.Version);
                }
            }
            catch (OperationCanceledException ex) when (cts.IsCancellationRequested)
            {
                throw new WebException("Http request timed out", ex.InnerException, WebExceptionStatus.Timeout, null);
            }
        }

        protected virtual System.Net.Http.HttpClient GetClient(HttpUri uri)
        {
            var proxySettings = _proxySettingsProvider.GetProxySettings(uri);

            var key = proxySettings?.Key ?? NO_PROXY_KEY;

            return _httpClientCache.Get(key, () => CreateHttpClient(proxySettings));
        }

        protected virtual System.Net.Http.HttpClient CreateHttpClient(HttpProxySettings proxySettings)
        {
            var handler = new SocketsHttpHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Brotli,
                UseCookies = false, // sic - we don't want to use a shared cookie container
                AllowAutoRedirect = false,
                Credentials = GetCredentialCache(),
                PreAuthenticate = true,
                MaxConnectionsPerServer = 12,
                ConnectCallback = onConnect,
                SslOptions = new SslClientAuthenticationOptions
                {
                    RemoteCertificateValidationCallback = _certificateValidationService.ShouldByPassValidationError
                }
            };

            if (proxySettings != null)
            {
                handler.Proxy = _createManagedWebProxy.GetWebProxy(proxySettings);
            }

            var client = new System.Net.Http.HttpClient(handler)
            {
                DefaultRequestVersion = HttpVersion.Version20,
                DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower,
                Timeout = Timeout.InfiniteTimeSpan
            };

            return client;
        }

        protected virtual void AddRequestHeaders(HttpRequestMessage webRequest, HttpHeader headers)
        {
            foreach (var header in headers)
            {
                switch (header.Key)
                {
                    case "Accept":
                        webRequest.Headers.Accept.ParseAdd(header.Value);
                        break;
                    case "Connection":
                        webRequest.Headers.Connection.Clear();
                        webRequest.Headers.Connection.Add(header.Value);
                        break;
                    case "Content-Length":
                        AddContentHeader(webRequest, "Content-Length", header.Value);
                        break;
                    case "Content-Type":
                        AddContentHeader(webRequest, "Content-Type", header.Value);
                        break;
                    case "Date":
                        webRequest.Headers.Remove("Date");
                        webRequest.Headers.Date = HttpHeader.ParseDateTime(header.Value);
                        break;
                    case "Expect":
                        webRequest.Headers.Expect.ParseAdd(header.Value);
                        break;
                    case "Host":
                        webRequest.Headers.Host = header.Value;
                        break;
                    case "If-Modified-Since":
                        webRequest.Headers.IfModifiedSince = HttpHeader.ParseDateTime(header.Value);
                        break;
                    case "Range":
                        throw new NotImplementedException();
                    case "Referer":
                        webRequest.Headers.Add("Referer", header.Value);
                        break;
                    case "Transfer-Encoding":
                        webRequest.Headers.TransferEncoding.ParseAdd(header.Value);
                        break;
                    case "User-Agent":
                        webRequest.Headers.UserAgent.ParseAdd(header.Value);
                        break;
                    case "Proxy-Connection":
                        throw new NotImplementedException();
                    default:
                        webRequest.Headers.Add(header.Key, header.Value);
                        break;
                }
            }
        }

        private static void AddContentHeader(HttpRequestMessage request, string header, string value)
        {
            var headers = request.Content?.Headers;
            if (headers == null)
            {
                return;
            }

            headers.Remove(header);
            headers.Add(header, value);
        }

        private CredentialCache GetCredentialCache()
        {
            return _credentialCache.Get("credentialCache", () => new CredentialCache());
        }

        private static bool HasRoutableIPv4Address()
        {
            // Get all IPv4 addresses from all interfaces and return true if there are any with non-loopback addresses
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            return networkInterfaces.Any(ni =>
                ni.OperationalStatus == OperationalStatus.Up &&
                ni.GetIPProperties().UnicastAddresses.Any(ip =>
                    ip.Address.AddressFamily == AddressFamily.InterNetwork &&
                    !IPAddress.IsLoopback(ip.Address)));
        }

        private static async ValueTask<Stream> onConnect(SocketsHttpConnectionContext context, CancellationToken cancellationToken)
        {
            // Until .NET supports an implementation of Happy Eyeballs (https://tools.ietf.org/html/rfc8305#section-2), let's make IPv4 fallback work in a simple way.
            // This issue is being tracked at https://github.com/dotnet/runtime/issues/26177 and expected to be fixed in .NET 6.
            if (useIPv6)
            {
                try
                {
                    var localToken = cancellationToken;

                    if (!hasResolvedIPv6Availability)
                    {
                        // to make things move fast, use a very low timeout for the initial ipv6 attempt.
                        var quickFailCts = new CancellationTokenSource(connection_establish_timeout);
                        var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, quickFailCts.Token);

                        localToken = linkedTokenSource.Token;
                    }

                    return await attemptConnection(AddressFamily.InterNetworkV6, context, localToken);
                }
                catch
                {
                    // Do not retry IPv6 if a routable IPv4 address is available, otherwise continue to attempt IPv6 connections.
                    useIPv6 = !HasRoutableIPv4Address();
                }
                finally
                {
                    hasResolvedIPv6Availability = true;
                }
            }

            // fallback to IPv4.
            return await attemptConnection(AddressFamily.InterNetwork, context, cancellationToken);
        }

        private static async ValueTask<Stream> attemptConnection(AddressFamily addressFamily, SocketsHttpConnectionContext context, CancellationToken cancellationToken)
        {
            // The following socket constructor will create a dual-mode socket on systems where IPV6 is available.
            var socket = new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp)
            {
                // Turn off Nagle's algorithm since it degrades performance in most HttpClient scenarios.
                NoDelay = true
            };

            try
            {
                await socket.ConnectAsync(context.DnsEndPoint, cancellationToken).ConfigureAwait(false);

                // The stream should take the ownership of the underlying socket,
                // closing it when it's disposed.
                return new NetworkStream(socket, ownsSocket: true);
            }
            catch
            {
                socket.Dispose();
                throw;
            }
        }
    }
}
