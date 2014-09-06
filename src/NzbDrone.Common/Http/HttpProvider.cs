using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using NLog;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Common.Http
{
    public interface IHttpProvider
    {
        string DownloadString(string url);
        string DownloadString(string url, string username, string password);
        Stream DownloadStream(string url, NetworkCredential credential = null);
    }

    public class HttpProvider : IHttpProvider
    {
        private readonly Logger _logger;

        public const string CONTENT_LENGTH_HEADER = "Content-Length";

        private readonly string _userAgent;

        public HttpProvider(Logger logger)
        {
            _logger = logger;
            _userAgent = String.Format("NzbDrone {0}", BuildInfo.Version);
            ServicePointManager.Expect100Continue = false;
        }

        public string DownloadString(string url)
        {
            return DownloadString(url, null);
        }

        public string DownloadString(string url, string username, string password)
        {
            return DownloadString(url, new NetworkCredential(username, password));
        }

        private string DownloadString(string url, ICredentials identity)
        {
            try
            {
                var client = new GZipWebClient { Credentials = identity };
                client.Headers.Add(HttpRequestHeader.UserAgent, _userAgent);
                return client.DownloadString(url);
            }
            catch (WebException e)
            {
                _logger.Warn("Failed to get response from: {0} {1}", url, e.Message);
                throw;
            }
            catch (Exception e)
            {
                _logger.WarnException("Failed to get response from: " + url, e);
                throw;
            }
        }

        public Stream DownloadStream(string url, NetworkCredential credential = null)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            request.UserAgent = _userAgent;
            request.Timeout = 20 * 1000;

            request.Credentials = credential;
            var response = request.GetResponse();

            return response.GetResponseStream();
        }

    
    }
}