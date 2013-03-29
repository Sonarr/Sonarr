using System;
using System.Collections.Generic;
using Autofac;
using NLog;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Autofac;
using Nancy.Conventions;
using Nancy.Diagnostics;
using NzbDrone.Api.ErrorManagement;
using NzbDrone.Api.Extensions;
using NzbDrone.Api.Frontend;
using NzbDrone.Common;
using NzbDrone.Core;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Lifecycle;
using SignalR;
using ErrorPipeline = NzbDrone.Api.ErrorManagement.ErrorPipeline;

namespace NzbDrone.Api
{

    public class NancyBootstrapper : AutofacNancyBootstrapper
    {
        private readonly Logger _logger;

        public NancyBootstrapper()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        protected override Nancy.IRootPathProvider RootPathProvider
        {
            get
            {
                return new RootPathProvider();
            }
        }

        protected override void ApplicationStartup(ILifetimeScope container, IPipelines pipelines)
        {
            _logger.Info("Starting NzbDrone API");
            AutomapperBootstraper.InitializeAutomapper();
            SignalRBootstraper.InitializeAutomapper(container);
            RegisterReporting(container);
            KickoffInitilizables(container);

            ApplicationPipelines.OnError.AddItemToEndOfPipeline(container.Resolve<ErrorPipeline>().HandleException);
        }

        private void KickoffInitilizables(ILifetimeScope container)
        {
            var initilizables = container.Resolve<IEnumerable<IInitializable>>();

            foreach (var initializable in initilizables)
            {
                _logger.Debug("Initializing {0}", initializable.GetType().Name);
                try
                {
                    initializable.Init();
                }
                catch (Exception e)
                {
                    _logger.FatalException("An error occurred while initializing " + initializable.GetType().Name, e);
                    throw;
                }
            }
        }

        private void RegisterReporting(ILifetimeScope container)
        {
            EnvironmentProvider.UGuid = container.Resolve<ConfigService>().UGuid;
            ReportingService.RestProvider = container.Resolve<RestProvider>();
        }

        protected override ILifetimeScope GetApplicationContainer()
        {
            _logger.Debug("Initializing Service Container");

            var builder = new ContainerBuilder();
            builder.RegisterCoreServices();
            builder.RegisterApiServices();

            var container = builder.Build();

            return container;
        }

        protected override NancyInternalConfiguration InternalConfiguration
        {
            get
            {
                var internalConfig = NancyInternalConfiguration.Default;

                internalConfig.StatusCodeHandlers.Add(typeof(ErrorHandler));
                internalConfig.Serializers.Add(typeof(NancyJsonSerializer));

                return internalConfig;
            }
        }

        protected override DiagnosticsConfiguration DiagnosticsConfiguration
        {
            get { return new DiagnosticsConfiguration { Password = @"password" }; }
        }

        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            base.ConfigureConventions(nancyConventions);
            var processors = ApplicationContainer.Resolve<IProcessStaticResource>();
            Conventions.StaticContentsConventions.Add(processors.ProcessStaticResourceRequest);
        }

    }

    public static class SignalRBootstraper
    {

        public static void InitializeAutomapper(ILifetimeScope container)
        {
            GlobalHost.DependencyResolver = new AutofacSignalrDependencyResolver(container.BeginLifetimeScope("SignalR"));
        }
    }
}