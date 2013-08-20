using NLog;
using Nancy.Bootstrapper;
using Nancy.Diagnostics;
using NzbDrone.Api.Authentication;
using NzbDrone.Api.ErrorManagement;
using NzbDrone.Api.Extensions;
using NzbDrone.Common.Messaging;
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
            pipelines.AfterRequest.AddItemToEndOfPipeline(CacheHeaderPipeline.Handle);

            ApplicationPipelines.OnError.AddItemToEndOfPipeline(container.Resolve<NzbDroneErrorPipeline>().HandleException);
        }


        protected override TinyIoCContainer GetApplicationContainer()
        {
            return _tinyIoCContainer;
        }


        protected override DiagnosticsConfiguration DiagnosticsConfiguration
        {
            get { return new DiagnosticsConfiguration { Password = @"password" }; }
        }

        protected override byte[] FavIcon
        {
            get
            {
                return null;
            }
        }
    }
}