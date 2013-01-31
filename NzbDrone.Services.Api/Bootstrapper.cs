using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using Autofac;
using NLog;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Autofac;
using NzbDrone.Services.Api.Extensions;

namespace NzbDrone.Services.Api
{
    public class Bootstrapper : AutofacNancyBootstrapper
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected override ILifetimeScope GetApplicationContainer()
        {
            ApplicationPipelines.OnError.AddItemToEndOfPipeline((context, exception) =>
            {
                Logger.FatalException("Application Exception", exception);
                return null;
            });

            var builder = new ContainerBuilder();
            builder.RegisterApiServices();
            var container = builder.Build();

            return container;
        }

        protected override NancyInternalConfiguration InternalConfiguration
        {
            get
            {
                return NancyInternalConfiguration.WithOverrides(c => c.Serializers.Add(typeof(NancyJsonSerializer)));
            }
        }
    }
}