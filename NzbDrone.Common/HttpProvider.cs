using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using NLog;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Common
{
    public interface IHttpProvider
    {
        string DownloadString(string url);
        string DownloadString(string url, string username, string password);
        Dictionary<string, string> GetHeader(string url);

        Stream DownloadStream(string url, NetworkCredential credential = null);
        void DownloadFile(string url, string fileName);
        string PostCommand(string address, string username, string password, string command);
    }

    public class HttpProvider : IHttpProvider
    {
        private readonly Logger _logger;

        public const string CONTENT_LENGHT_HEADER = "Content-Length";

        private readonly string _userAgent;

        public HttpProvider(Logger logger)
        {
            _logger = logger;
            _userAgent = String.Format("NzbDrone {0}", BuildInfo.Version);
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
                var client = new WebClient { Credentials = identity };
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

        public Dictionary<string, string> GetHeader(string url)
        {
            var headers = new Dictionary<string, string>();
            var request = WebRequest.Create(url);
            request.Method = "HEAD";

            var response = request.GetResponse();

            foreach (var key in response.Headers.AllKeys)
            {
                headers.Add(key, response.Headers[key]);
            }

            return headers;
        }

        public Stream DownloadStream(string url, NetworkCredential credential = null)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent = _userAgent;
            request.Timeout = 20 * 1000;

            request.Credentials = credential;
            var response = request.GetResponse();

            return response.GetResponseStream();
        }

        public void DownloadFile(string url, string fileName)
        {
            try
            {
                var fileInfo = new FileInfo(fileName);
                if (fileInfo.Directory != null && !fileInfo.Directory.Exists)
                {
                    fileInfo.Directory.Create();
                }

                _logger.Trace("Downloading [{0}] to [{1}]", url, fileName);

                var stopWatch = Stopwatch.StartNew();
                var webClient = new WebClient();
                webClient.Headers.Add(HttpRequestHeader.UserAgent, _userAgent);
                webClient.DownloadFile(url, fileName);
                stopWatch.Stop();
                _logger.Trace("Downloading Completed. took {0:0}s", stopWatch.Elapsed.Seconds);
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

        public string PostCommand(string address, string username, string password, string command)
        {
            address = String.Format("http://{0}/jsonrpc", address);

            _logger.Trace("Posting command: {0}, to {1}", command, address);

            byte[] byteArray = Encoding.ASCII.GetBytes(command);

            var wc = new WebClient();
            wc.Credentials = new NetworkCredential(username, password);
            var response = wc.UploadData(address, "POST", byteArray);
            var text = Encoding.ASCII.GetString(response);

            return text.Replace("&nbsp;", " ");
        }
    }
}