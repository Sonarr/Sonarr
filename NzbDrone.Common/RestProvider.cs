using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using NzbDrone.Common.Contract;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Common
{

    public class RestProvider
    {
        private const int TIMEOUT = 15000;
        private const string METHOD = "POST";

        public virtual void PostData(string url, ReportBase reportBase)
        {
            reportBase.Version = BuildInfo.Version.ToString();
            reportBase.IsProduction = RuntimeInfo.IsProduction;

            PostData(url, reportBase as object);
        }


        private static void PostData(string url, object message)
        {
            try
            {
                var json = JsonConvert.SerializeObject(message);

                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Timeout = TIMEOUT;

                request.Proxy = WebRequest.DefaultWebProxy;

                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.Method = METHOD;
                request.ContentType = "application/json";

                byte[] postBytes = Encoding.UTF8.GetBytes(json);
                request.ContentLength = postBytes.Length;

                var requestStream = request.GetRequestStream();
                requestStream.Write(postBytes, 0, postBytes.Length);
                requestStream.Close();

                var response = (HttpWebResponse)request.GetResponse();
                var streamreader = new StreamReader(response.GetResponseStream());
                streamreader.Close();
            }
            catch (Exception e)
            {
                e.Data.Add("URL", url);
                throw;
            }
        }
    }
}
