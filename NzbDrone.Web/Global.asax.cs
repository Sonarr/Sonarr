using System;
using System.Diagnostics;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Ninject;
using Ninject.Web.Mvc;
using NLog;
using NzbDrone.Core;

namespace NzbDrone.Web
{
    public class MvcApplication : NinjectHttpApplication
    {

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{*robotstxt}", new { robotstxt = @"(.*/)?robots.txt(/.*)?" });
            routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.ico(/.*)?" });

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Series", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
        }

        protected override void OnApplicationStarted()
        {
            Instrumentation.Setup();
            AreaRegistration.RegisterAllAreas();
            RegisterRoutes(RouteTable.Routes);
            base.OnApplicationStarted();
        }

        protected override IKernel CreateKernel()
        {
            return CentralDispatch.NinjectKernel;
        }

        // ReSharper disable InconsistentNaming
        protected void Application_Error(object sender, EventArgs e)
        {
            Instrumentation.LogEpicException(Server.GetLastError());
        }



    }
}