using System;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using LowercaseRoutesMVC;
using NLog;
using NzbDrone.Core;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Web.Helpers.Binders;
using SignalR;

namespace NzbDrone.Web
{
    public class MvcApplication : HttpApplication
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("api/{*pathInfo}");
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{resource}.html");
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

        protected void Application_Start()
        {
            InitContainer();

            RegisterRoutes(RouteTable.Routes);
            AreaRegistration.RegisterAllAreas();

            var razor = ViewEngines.Engines.Single(e => e is RazorViewEngine);
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(razor);

            ModelBinders.Binders.Add(typeof(QualityTypes), new QualityTypesBinder());

            RegisterGlobalFilters(GlobalFilters.Filters);

            Logger.Info("Fully initialized and ready.");
        }

        private void InitContainer()
        {
            Logger.Info("NzbDrone Starting up.");
            var dispatch = new CentralDispatch();
            dispatch.DedicateToHost();

            dispatch.ContainerBuilder.RegisterAssemblyTypes(typeof(MvcApplication).Assembly).SingleInstance();
            dispatch.ContainerBuilder.RegisterAssemblyTypes(typeof(MvcApplication).Assembly).AsImplementedInterfaces().SingleInstance();

            MVCRegistration(dispatch.ContainerBuilder);

            var container = dispatch.BuildContainer();

            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            //SignalR
            RouteTable.Routes.MapHubs();

        }

        private static void MVCRegistration(ContainerBuilder builder)
        {
            builder.RegisterModule(new AutofacWebTypesModule());

            builder.RegisterControllers(typeof(MvcApplication).Assembly).InjectActionInvoker();
            builder.RegisterModelBinders(typeof(MvcApplication).Assembly).SingleInstance();

            builder.RegisterType<ControllerActionInvoker>().As<IActionInvoker>();
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