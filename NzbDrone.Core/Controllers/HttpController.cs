using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace NzbDrone.Core.Controllers
{
    class HttpController : IHttpController
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
