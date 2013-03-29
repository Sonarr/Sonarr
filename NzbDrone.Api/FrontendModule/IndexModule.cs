using System;
using System.Linq;
using Nancy;
using Nancy.Responses.Negotiation;

namespace NzbDrone.Api.FrontendModule
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
                || Request.Path.StartsWith("/api", StringComparison.CurrentCultureIgnoreCase))
            {
                return new NotFoundResponse();
            }


            return View["UI/index.html"];
        }
    }
}