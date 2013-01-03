using System;
using System.Data.Common;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using NLog;
using NzbDrone.Core;

namespace NzbDrone.Services.Service
{
    public class MvcApplication : HttpApplication
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                            "Default", // Route name
                            "{controller}/{action}", // URL with parameters
                            new { controller = "Health", action = "Echo" } // Parameter default
         );

        }

        private static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            var razor = ViewEngines.Engines.Single(e => e is RazorViewEngine);
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(razor);

            ModelBinders.Binders.DefaultBinder = new JsonModelBinder();

            InitContainer();
        }

        // ReSharper disable InconsistentNaming
        protected void Application_Error(object sender, EventArgs e)
        {
            var lastError = Server.GetLastError();

            if (lastError is HttpException && lastError.InnerException == null)
            {
                logger.WarnException(String.Format("{0}. URL[{1}]", lastError.Message, Request.Path), lastError);
                return;
            }

            logger.FatalException(lastError.Message + Environment.NewLine + Request.Url.PathAndQuery, lastError);
        }

        protected void Application_BeginRequest()
        {
        }

        protected void Application_EndRequest()
        {
        }

        private void InitContainer()
        {
            logger.Info("NzbDrone Starting up.");
            var dispatch = new CentralDispatch();
            

            dispatch.ContainerBuilder.RegisterAssemblyTypes(typeof(MvcApplication).Assembly).SingleInstance();
            dispatch.ContainerBuilder.RegisterAssemblyTypes(typeof(MvcApplication).Assembly).AsImplementedInterfaces().SingleInstance();

            MVCRegistration(dispatch.ContainerBuilder);

            var container = dispatch.ContainerBuilder.Build();

            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }

        private static void MVCRegistration(ContainerBuilder builder)
        {
            builder.RegisterModule(new AutofacWebTypesModule());

            builder.RegisterControllers(typeof(MvcApplication).Assembly).InjectActionInvoker();
            builder.RegisterModelBinders(typeof(MvcApplication).Assembly).SingleInstance();

            builder.RegisterType<ControllerActionInvoker>().As<IActionInvoker>();
        }
    }
}