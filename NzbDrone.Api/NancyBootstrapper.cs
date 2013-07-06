using System;
using NLog;
using Nancy;
using Nancy.Authentication.Basic;
using Nancy.Bootstrapper;
using Nancy.Conventions;
using Nancy.Diagnostics;
using NzbDrone.Api.Authentication;
using NzbDrone.Api.ErrorManagement;
using NzbDrone.Api.Extensions;
using NzbDrone.Api.Frontend;
using NzbDrone.Common;
using NzbDrone.Common.Composition;
using NzbDrone.Common.Messaging;
using NzbDrone.Common.Model;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Lifecycle;
using TinyIoC;

namespace NzbDrone.Api
{
    public class NancyBootstrapper : TinyIoCNancyBootstrapper
    {
        private readonly TinyIoCContainer _tinyIoCContainer;
        private readonly Logger _logger;

        public NancyBootstrapper(TinyIoCContainer tinyIoCContainer)
        {
            _tinyIoCContainer = tinyIoCContainer;
            _logger = LogManager.GetCurrentClassLogger();
        }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            _logger.Info("Starting NzbDrone API");

            container.Resolve<DatabaseTarget>().Register();
            container.Resolve<IEnableBasicAuthInNancy>().Register(pipelines);
            container.Resolve<IMessageAggregator>().PublishEvent(new ApplicationStartedEvent());

            pipelines.AfterRequest.AddItemToStartOfPipeline(GzipCompressionPipeline.Handle);

            ApplicationPipelines.OnError.AddItemToEndOfPipeline(container.Resolve<NzbDroneErrorPipeline>().HandleException);
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

        protected override byte[] FavIcon
        {
            get
            {
                return null;
            }
        }

        public void Shutdown()
        {
            ApplicationContainer.Resolve<IMessageAggregator>().PublishEvent(new ApplicationShutdownRequested());
        }
    }
}