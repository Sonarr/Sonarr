using System;
using System.Net;

namespace NzbDrone.Core.Providers
{
    class HttpProvider : IHttpProvider
    {
        public string GetRequest(string request)
        {
            //Get the request and return as String Array
            try
            {
                var webClient = new WebClient();
                return webClient.DownloadString(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return String.Empty;
            }            
        }
    }
}
