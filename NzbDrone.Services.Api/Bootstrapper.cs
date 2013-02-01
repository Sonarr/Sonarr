using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using AutoMapper;
using Autofac;
using MongoDB.Bson;
using NLog;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Autofac;
using NzbDrone.Services.Api.Extensions;
using NzbDrone.Services.Api.SceneMapping;

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

        protected override void ApplicationStartup(ILifetimeScope container, IPipelines pipelines)
        {
            InitializeAutomapper();
        }

        public static void InitializeAutomapper()
        {
            Mapper.CreateMap<SceneMappingModel, SceneMappingJsonModel>()
                  .ForMember(dest => dest.MapId, opt => opt.MapFrom(src => src.MapId.ToString()));

            Mapper.CreateMap<SceneMappingJsonModel, SceneMappingModel>()
                  .ForMember(dest => dest.MapId, opt => opt.MapFrom(src => ObjectId.Parse(src.MapId)));
        }
    }
}