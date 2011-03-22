using System;
using System.Net;
using NLog;

namespace NzbDrone.Core.Providers
{
    internal class HttpProvider : IHttpProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public string DownloadString(string request)
        {
            try
            {
                return new WebClient().DownloadString(request);
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to get response from: {0}", request);
                Logger.TraceException(ex.Message, ex);
            }

            return String.Empty;
        }

        public string DownloadString(string request, string username, string password)
        {
            try
            {
                var webClient = new WebClient();
                webClient.Credentials = new NetworkCredential(username, password);
                return webClient.DownloadString(request);
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to get response from: {0}", request);
                Logger.TraceException(ex.Message, ex);
            }

            return String.Empty;
        }

        public bool DownloadFile(string request, string filename)
        {
            try
            {
                var webClient = new WebClient();
                webClient.DownloadFile(request, filename);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to get response from: {0}", request);
                Logger.TraceException(ex.Message, ex);
            }

            return false;
        }

        public bool DownloadFile(string request, string filename, string username, string password)
        {
            try
            {
                var webClient = new WebClient();
                webClient.Credentials = new NetworkCredential(username, password);
                webClient.DownloadFile(request, filename);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to get response from: {0}", request);
                Logger.TraceException(ex.Message, ex);
            }

            return false;
        }
    }
}