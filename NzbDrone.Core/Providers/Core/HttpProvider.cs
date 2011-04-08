using System;
using System.Net;
using System.Xml;
using NLog;

namespace NzbDrone.Core.Providers.Core
{
    public class HttpProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public virtual string DownloadString(string request)
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

        public virtual string DownloadString(string request, string username, string password)
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

        public virtual void DownloadFile(string request, string filename)
        {
            try
            {
                var webClient = new WebClient();
                webClient.DownloadFile(request, filename);

            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to get response from: {0}", request);
                Logger.TraceException(ex.Message, ex);
                throw;
            }


        }

        public virtual void DownloadFile(string request, string filename, string username, string password)
        {
            try
            {
                var webClient = new WebClient();
                webClient.Credentials = new NetworkCredential(username, password);
                webClient.DownloadFile(request, filename);
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to get response from: {0}", request);
                Logger.TraceException(ex.Message, ex);
                throw;
            }
        }

        public virtual XmlReader DownloadXml(string url)
        {
            return XmlReader.Create(url);
        }
    }
}