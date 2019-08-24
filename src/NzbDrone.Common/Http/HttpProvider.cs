using System;
using System.Net;
using NLog;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Common.Http
{
    [Obsolete("Use IHttpClient")]
    public interface IHttpProvider
    {
        string DownloadString(string url);
        string DownloadString(string url, string username, string password);
    }


    [Obsolete("Use HttpProvider")]
    public class HttpProvider : IHttpProvider
    {
        private readonly Logger _logger;


        private readonly string _userAgent;

        public HttpProvider(Logger logger)
        {
            _logger = logger;
            _userAgent = $"{BuildInfo.AppName}/{BuildInfo.Version.ToString(2)}";
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
                _logger.Warn(e, "Failed to get response from: " + url);
                throw;
            }
        }

    
    }
}