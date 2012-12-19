using System;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using LowercaseRoutesMVC;
using NLog.Config;
using Ninject;
using Ninject.Web.Common;
using NLog;
using NzbDrone.Api;
using NzbDrone.Common;
using NzbDrone.Core;
using ServiceStack.CacheAccess;
using ServiceStack.CacheAccess.Providers;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Web.Helpers.Binders;
using ServiceStack.ServiceInterface;
using SignalR;

namespace NzbDrone.Web
{
    public class MvcApplication : NinjectHttpApplication
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("api/{*pathInfo}");
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{*robotstxt}", new { robotstxt = @"(.*/)?robots.txt(/.*)?" });
            routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.ico(/.*)?" });
            
            routes.MapRouteLowercase(
                name: "WithSeasonNumber",
                url: "{controller}/{action}/{seriesId}/{seasonNumber}"
            );

            routes.MapRouteLowercase(
                name: "SeriesId",
                url: "{controller}/{action}/{seriesId}",
                defaults: new { controller = "Series", action = "Index", seriesId = UrlParameter.Optional }
            );
        }

        protected override void OnApplicationStarted()
        {
            base.OnApplicationStarted();

            RegisterRoutes(RouteTable.Routes);
            AreaRegistration.RegisterAllAreas();

            var razor = ViewEngines.Engines.Single(e => e is RazorViewEngine);
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(razor);

            ModelBinders.Binders.Add(typeof(QualityTypes), new QualityTypesBinder());

            RegisterGlobalFilters(GlobalFilters.Filters);

            Logger.Info("Fully initialized and ready.");
        }

        protected override IKernel CreateKernel()
        {
            Logger.Info("NzbDrone Starting up.");
            var dispatch = new CentralDispatch();
            dispatch.DedicateToHost();

            dispatch.Kernel.Load(Assembly.GetExecutingAssembly());

            //SignalR
            RouteTable.Routes.MapHubs();

            //ServiceStack
            dispatch.Kernel.Bind<ICacheClient>().To<MemoryCacheClient>().InSingletonScope();
            dispatch.Kernel.Bind<ISessionFactory>().To<SessionFactory>().InSingletonScope();
            new AppHost(dispatch.Kernel).Init();

            return dispatch.Kernel;
        }

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        // ReSharper disable InconsistentNaming
        protected void Application_Error(object sender, EventArgs e)
        {
            var lastError = Server.GetLastError();

            if (lastError is HttpException && lastError.InnerException == null)
            {
                Logger.WarnException(String.Format("{0}. URL[{1}]", lastError.Message, Request.Path), lastError);
                return;
            }

            Logger.FatalException(lastError.Message + Environment.NewLine + Request.Url.PathAndQuery, lastError);

            if (lastError is DbException)
            {
                Logger.Warn("Restarting application");
                HttpRuntime.UnloadAppDomain();
            }
        }

        protected void Application_BeginRequest()
        {
            Thread.CurrentThread.Name = "WEB_THREAD";
        }

        protected void Application_EndRequest()
        {
        }
    }
}