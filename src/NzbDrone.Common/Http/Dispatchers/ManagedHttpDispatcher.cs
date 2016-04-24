using System;
using System.Net;
using NzbDrone.Common.Extensions;
using com.LandonKey.SocksWebProxy.Proxy;
using com.LandonKey.SocksWebProxy;
using System.Net.Sockets;
using System.Linq;

namespace NzbDrone.Common.Http.Dispatchers
{
    public class ManagedHttpDispatcher : IHttpDispatcher
    {
        public HttpResponse GetResponse(HttpRequest request, CookieContainer cookies)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create((Uri)request.Url);

            // Deflate is not a standard and could break depending on implementation.
            // we should just stick with the more compatible Gzip
            //http://stackoverflow.com/questions/8490718/how-to-decompress-stream-deflated-with-java-util-zip-deflater-in-net
            webRequest.AutomaticDecompression = DecompressionMethods.GZip;

            webRequest.Method = request.Method.ToString();
            webRequest.UserAgent = UserAgentBuilder.UserAgent;
            webRequest.KeepAlive = request.ConnectionKeepAlive;
            webRequest.AllowAutoRedirect = request.AllowAutoRedirect;
            webRequest.CookieContainer = cookies;

            if (request.RequestTimeout != TimeSpan.Zero)
            {
                webRequest.Timeout = (int)Math.Ceiling(request.RequestTimeout.TotalMilliseconds);
            }

            if (request.Proxy != null && !request.Proxy.ShouldProxyBeBypassed(new Uri(request.Url.FullUri)))
            {
                var addresses = Dns.GetHostAddresses(request.Proxy.Host);

                if(addresses.Length > 1)
                {
                    var ipv4Only = addresses.Where(a => a.AddressFamily == AddressFamily.InterNetwork);
                    if (ipv4Only.Any())
                    {
                        addresses = ipv4Only.ToArray();
                    }
                }

                var socksUsername = request.Proxy.Username == null ? string.Empty : request.Proxy.Username;
                var socksPassword = request.Proxy.Password == null ? string.Empty : request.Proxy.Password;

                switch (request.Proxy.Type)
                {
                    case ProxyType.Http:
                        if(request.Proxy.Username.IsNotNullOrWhiteSpace() && request.Proxy.Password.IsNotNullOrWhiteSpace())
                        {
                            webRequest.Proxy = new WebProxy(request.Proxy.Host + ":" + request.Proxy.Port, request.Proxy.BypassLocalAddress, request.Proxy.SubnetFilterAsArray, new NetworkCredential(request.Proxy.Username, request.Proxy.Password));
                        }
                        else
                        {
                            webRequest.Proxy = new WebProxy(request.Proxy.Host + ":" + request.Proxy.Port, request.Proxy.BypassLocalAddress, request.Proxy.SubnetFilterAsArray);
                        }
                        break;
                    case ProxyType.Socks4:
                        webRequest.Proxy = new SocksWebProxy(new ProxyConfig(IPAddress.Parse("127.0.0.1"), GetNextFreePort(), addresses[0], request.Proxy.Port, ProxyConfig.SocksVersion.Four, socksUsername, socksPassword), false);
                        break;
                    case ProxyType.Socks5:
                        webRequest.Proxy = new SocksWebProxy(new ProxyConfig(IPAddress.Parse("127.0.0.1"), GetNextFreePort(), addresses[0], request.Proxy.Port, ProxyConfig.SocksVersion.Five, socksUsername, socksPassword), false);
                        break;
                }                
            }

            if (request.Headers != null)
            {
                AddRequestHeaders(webRequest, request.Headers);
            }

            if (request.ContentData != null)
            {
                webRequest.ContentLength = request.ContentData.Length;
                using (var writeStream = webRequest.GetRequestStream())
                {
                    writeStream.Write(request.ContentData, 0, request.ContentData.Length);
                }
            }

            HttpWebResponse httpWebResponse;

            try
            {
                httpWebResponse = (HttpWebResponse)webRequest.GetResponse();
            }
            catch (WebException e)
            {
                httpWebResponse = (HttpWebResponse)e.Response;

                if (httpWebResponse == null)
                {
                    throw;
                }
            }

            byte[] data = null;

            using (var responseStream = httpWebResponse.GetResponseStream())
            {
                if (responseStream != null)
                {
                    data = responseStream.ToBytes();
                }
            }

            return new HttpResponse(request, new HttpHeader(httpWebResponse.Headers), data, httpWebResponse.StatusCode);
        }

        protected virtual void AddRequestHeaders(HttpWebRequest webRequest, HttpHeader headers)
        {
            foreach (var header in headers)
            {
                switch (header.Key)
                {
                    case "Accept":
                        webRequest.Accept = header.Value;
                        break;
                    case "Connection":
                        webRequest.Connection = header.Value;
                        break;
                    case "Content-Length":
                        webRequest.ContentLength = Convert.ToInt64(header.Value);
                        break;
                    case "Content-Type":
                        webRequest.ContentType = header.Value;
                        break;
                    case "Date":
                        webRequest.Date = HttpHeader.ParseDateTime(header.Value);
                        break;
                    case "Expect":
                        webRequest.Expect = header.Value;
                        break;
                    case "Host":
                        webRequest.Host = header.Value;
                        break;
                    case "If-Modified-Since":
                        webRequest.IfModifiedSince = HttpHeader.ParseDateTime(header.Value);
                        break;
                    case "Range":
                        throw new NotImplementedException();
                    case "Referer":
                        webRequest.Referer = header.Value;
                        break;
                    case "Transfer-Encoding":
                        webRequest.TransferEncoding = header.Value;
                        break;
                    case "User-Agent":
                        throw new NotSupportedException("User-Agent other than Sonarr not allowed.");
                    case "Proxy-Connection":
                        throw new NotImplementedException();
                    default:
                        webRequest.Headers.Add(header.Key, header.Value);
                        break;
                }
            }
        }

        private static int GetNextFreePort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();

            return port;
        }
    }
}
