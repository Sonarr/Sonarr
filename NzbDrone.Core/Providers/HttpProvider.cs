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
    }
}