using NLog;
using Nancy.Bootstrapper;
using Nancy.Conventions;
using Nancy.Diagnostics;
using NzbDrone.Api.ErrorManagement;
using NzbDrone.Api.Extensions;
using NzbDrone.Api.Frontend;
using NzbDrone.Common;
using NzbDrone.Common.Eventing;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Lifecycle;
using TinyIoC;

namespace NzbDrone.Api
{
    public class TinyNancyBootstrapper : TinyIoCNancyBootstrapper
    {
        private readonly TinyIoCContainer _tinyIoCContainer;
        private readonly Logger _logger;

        public TinyNancyBootstrapper(TinyIoCContainer tinyIoCContainer)
        {
            _tinyIoCContainer = tinyIoCContainer;
            _logger = LogManager.GetCurrentClassLogger();
        }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            _logger.Info("Starting NzbDrone API");
            AutomapperBootstraper.InitializeAutomapper();
            RegisterReporting(container);

            container.Resolve<IEventAggregator>().Publish(new ApplicationStartedEvent());

            ApplicationPipelines.OnError.AddItemToEndOfPipeline(container.Resolve<ErrorPipeline>().HandleException);
        }

        private void RegisterReporting(TinyIoCContainer container)
        {
            EnvironmentProvider.UGuid = container.Resolve<ConfigService>().UGuid;
            ReportingService.RestProvider = container.Resolve<RestProvider>();
        }

        protected override TinyIoCContainer GetApplicationContainer()
        {
            return _tinyIoCContainer;
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
}