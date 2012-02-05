using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using NLog;

namespace NzbDrone.Core.Providers.Core
{
    public class HttpProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public virtual string DownloadString(string address)
        {
            try
            {
                return new WebClient().DownloadString(address);
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to get response from: {0}", address);
                Logger.TraceException(ex.Message, ex);
                throw;
            }
        }

        public virtual string DownloadString(string address, string username, string password)
        {
            try
            {
                var webClient = new WebClient();
                webClient.Credentials = new NetworkCredential(username, password);
                return webClient.DownloadString(address);
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to get response from: {0}", address);
                Logger.TraceException(ex.Message, ex);
                throw;
            }
        }

        public virtual Stream DownloadStream(string url, NetworkCredential credential)
        {
            var request = WebRequest.Create(url);

            request.Credentials = credential;
            var response = request.GetResponse();

            return response.GetResponseStream();
        }

        public virtual void DownloadFile(string url, string fileName)
        {
            try
            {
                var fileInfo = new FileInfo(fileName);
                if (!fileInfo.Directory.Exists)
                {
                    fileInfo.Directory.Create();
                }

                Logger.Trace("Downloading [{0}] to [{1}]", url, fileName);

                var stopWatch = Stopwatch.StartNew();
                var webClient = new WebClient();
                webClient.DownloadFile(url, fileName);
                stopWatch.Stop();
                Logger.Trace("Downloading Completed. took {0:0}s", stopWatch.Elapsed.Seconds);
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to get response from: {0}", url);
                Logger.TraceException(ex.Message, ex);
                throw;
            }
        }

        public virtual string PostCommand(string address, string username, string password, string command)
        {
            address = String.Format("http://{0}/jsonrpc", address);

            Logger.Trace("Posting command: {0}, to {1}", command, address);

            byte[] byteArray = Encoding.ASCII.GetBytes(command);

            var request = (HttpWebRequest)WebRequest.Create(address);
            request.Method = "POST";
            request.Credentials = new NetworkCredential(username, password);
            request.ContentType = "application/json";
            request.Timeout = 20000;
            request.KeepAlive = false;

            //Used to hold the JSON response
            string responseFromServer;

            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(byteArray, 0, byteArray.Length);

                using (var response = request.GetResponse())
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(responseStream))
                        {
                            responseFromServer = reader.ReadToEnd();
                        }
                    }
                }
            }

            return responseFromServer.Replace("&nbsp;", " ");
        }

        public virtual string Post(string address, string command, string username, string password)
        {
            Logger.Trace("Posting command: {0}, to {1}", command, address);

            byte[] byteArray = Encoding.ASCII.GetBytes(command);

            var request = (HttpWebRequest)WebRequest.Create(address);
            request.Method = "POST";
            request.Credentials = new NetworkCredential(username, password);
            request.ContentType = "application/json";
            request.Timeout = 2000;
            request.KeepAlive = false;

            //Used to hold the JSON response
            string responseFromServer;

            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(byteArray, 0, byteArray.Length);

                using (var response = request.GetResponse())
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(responseStream))
                        {
                            responseFromServer = reader.ReadToEnd();
                        }
                    }
                }
            }

            return responseFromServer.Replace("&nbsp;", " ");
        }
    }
}