using System;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Routing;
using MvcMiniProfiler;
using NLog.Config;
using Ninject;
using Ninject.Web.Mvc;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core;
using NzbDrone.Core.Instrumentation;
using Telerik.Web.Mvc;

namespace NzbDrone.Web
{
    public class MvcApplication : NinjectHttpApplication
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
            base.OnApplicationStarted();
            //WebAssetDefaultSettings.UseTelerikContentDeliveryNetwork = true;
            RegisterRoutes(RouteTable.Routes);
            //base.OnApplicationStarted();
            AreaRegistration.RegisterAllAreas();

            var razor = ViewEngines.Engines.Where(e => e.GetType() == typeof(RazorViewEngine)).Single();
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(razor);

            RegisterGlobalFilters(GlobalFilters.Filters);

            Logger.Info("Fully initialized and ready.");
        }

        protected override IKernel CreateKernel()
        {
            LogManager.Configuration = new XmlLoggingConfiguration(new EnviromentProvider().GetNlogConfigPath(), false);
            
            Common.LogConfiguration.RegisterUdpLogger();
            Common.LogConfiguration.RegisterExceptioneer();
            Common.LogConfiguration.RegisterConsoleLogger(LogLevel.Info, "NzbDrone.Web.MvcApplication");
            Common.LogConfiguration.RegisterConsoleLogger(LogLevel.Info, "NzbDrone.Core.CentralDispatch");
            

            var dispatch = new CentralDispatch();
            Logger.Info("NzbDrone Starting up.");

            dispatch.DedicateToHost();

            dispatch.Kernel.Load(Assembly.GetExecutingAssembly());
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
            Thread.CurrentThread.Name = "UI";
            var miniprofiler = MiniProfiler.Start();
        }

        protected void Application_EndRequest()
        {
            MiniProfiler.Stop();
        }
    }
}