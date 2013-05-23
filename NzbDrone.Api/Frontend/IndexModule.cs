using System;
using Nancy;
using Nancy.Security;

namespace NzbDrone.Api.Frontend
{
    public class IndexModule : NancyModule
    {
        public IndexModule()
        {
            //Serve anything that doesn't have an extension
            Get[@"/(.*)"] = x => Index();
        }

        private object Index()
        {
            if(
                Request.Path.Contains(".")
                || Request.Path.StartsWith("/static", StringComparison.CurrentCultureIgnoreCase) 
                || Request.Path.StartsWith("/api", StringComparison.CurrentCultureIgnoreCase)
                || Request.Path.StartsWith("/signalr", StringComparison.CurrentCultureIgnoreCase))
            {
                return new NotFoundResponse();
            }


            return View["UI/index.html"];
        }
    }
}