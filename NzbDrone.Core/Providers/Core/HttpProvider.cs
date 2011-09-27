using System;
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

        public virtual bool DownloadFile(string address, string fileName)
        {
            try
            {
                var webClient = new WebClient();
                webClient.DownloadFile(address, fileName);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to get response from: {0}", address);
                Logger.TraceException(ex.Message, ex);
                return false;
            }
        }

        public virtual string PostCommand(string address, string username, string password, string command)
        {
            address += "/jsonrpc";

            Logger.Trace("Posting command: {0}, to {1}", command, address);

            byte[] byteArray = Encoding.ASCII.GetBytes(command);

            var request = WebRequest.Create(address);
            request.Method = "POST";
            request.Credentials = new NetworkCredential(username, password);
            request.ContentLength = byteArray.Length;
            request.ContentType = "application/x-www-form-urlencoded";
            var dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            var response = request.GetResponse();
            dataStream = response.GetResponseStream();
            var reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();

            reader.Close();
            dataStream.Close();
            response.Close();

            return responseFromServer.Replace("&nbsp;", " ");
        }
    }
}