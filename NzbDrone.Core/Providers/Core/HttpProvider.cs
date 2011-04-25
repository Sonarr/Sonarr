using System;
using System.IO;
using System.Net;
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


    }
}