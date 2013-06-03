using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using NLog;

namespace NzbDrone.Common
{
    public interface IHttpProvider
    {
        string DownloadString(string address);
        string DownloadString(string address, string username, string password);
        string DownloadString(string address, ICredentials identity);
        Dictionary<string, string> GetHeader(string url);

        Stream DownloadStream(string url, NetworkCredential credential = null);
        void DownloadFile(string url, string fileName);
        string PostCommand(string address, string username, string password, string command);
    }

    public class HttpProvider : IHttpProvider
    {

        public const string ContentLenghtHeader = "Content-Length";

        private readonly IEnvironmentProvider _environmentProvider;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly string _userAgent;

        public HttpProvider(IEnvironmentProvider environmentProvider)
        {
            _environmentProvider = environmentProvider;
            _userAgent = String.Format("NzbDrone {0}", _environmentProvider.Version);
        }

        public string DownloadString(string address)
        {
            return DownloadString(address, null);
        }

        public string DownloadString(string address, string username, string password)
        {
            return DownloadString(address, new NetworkCredential(username, password));
        }

        public string DownloadString(string address, ICredentials identity)
        {
            try
            {
                var client = new WebClient { Credentials = identity };
                client.Headers.Add(HttpRequestHeader.UserAgent, _userAgent);
                return client.DownloadString(address);
            }
            catch (Exception ex)
            {
                logger.Trace(ex.Message, ex.ToString());
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

                logger.Trace("Downloading [{0}] to [{1}]", url, fileName);

                var stopWatch = Stopwatch.StartNew();
                var webClient = new WebClient();
                webClient.Headers.Add(HttpRequestHeader.UserAgent, _userAgent);
                webClient.DownloadFile(url, fileName);
                stopWatch.Stop();
                logger.Trace("Downloading Completed. took {0:0}s", stopWatch.Elapsed.Seconds);
            }
            catch (Exception ex)
            {
                logger.Warn("Failed to get response from: {0}", url);
                logger.TraceException(ex.Message, ex);
                throw;
            }
        }

        public string PostCommand(string address, string username, string password, string command)
        {
            address = String.Format("http://{0}/jsonrpc", address);

            logger.Trace("Posting command: {0}, to {1}", command, address);

            byte[] byteArray = Encoding.ASCII.GetBytes(command);

            var wc = new WebClient();
            wc.Credentials = new NetworkCredential(username, password);
            var response = wc.UploadData(address, "POST", byteArray);
            var text = Encoding.ASCII.GetString(response);

            return text.Replace("&nbsp;", " ");
        }
    }
}