using System;
using System.Linq;
using AutoMapper;
using Autofac;
using NLog;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Autofac;
using NzbDrone.Api.ErrorManagment;
using NzbDrone.Api.Extentions;
using NzbDrone.Api.QualityProfiles;
using NzbDrone.Api.QualityType;
using NzbDrone.Api.Resolvers;
using NzbDrone.Api.Series;
using NzbDrone.Core;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Repository.Quality;

namespace NzbDrone.Api
{

    public class Bootstrapper : AutofacNancyBootstrapper
    {
        private readonly Logger _logger;

        public Bootstrapper()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        protected override void ApplicationStartup(ILifetimeScope container, IPipelines pipelines)
        {
            InitializeAutomapper();
        }

        public static void InitializeAutomapper()
        {
            //QualityProfiles
            Mapper.CreateMap<QualityProfile, QualityProfileModel>()
                  .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.QualityProfileId))
                  .ForMember(dest => dest.Qualities,
                             opt => opt.ResolveUsing<AllowedToQualitiesResolver>().FromMember(src => src.Allowed));

            Mapper.CreateMap<QualityProfileModel, QualityProfile>()
                  .ForMember(dest => dest.QualityProfileId, opt => opt.MapFrom(src => src.Id))
                  .ForMember(dest => dest.Allowed,
                             opt => opt.ResolveUsing<QualitiesToAllowedResolver>().FromMember(src => src.Qualities));

            Mapper.CreateMap<QualityTypes, QualityProfileType>()
                  .ForMember(dest => dest.Allowed, opt => opt.Ignore());

            //QualityTypes
            Mapper.CreateMap<Core.Repository.Quality.QualityType, QualityTypeModel>()
                  .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.QualityTypeId));

            Mapper.CreateMap<QualityTypeModel, Core.Repository.Quality.QualityType>()
                  .ForMember(dest => dest.QualityTypeId, opt => opt.MapFrom(src => src.Id));

            //Series
            Mapper.CreateMap<Core.Repository.Series, SeriesModel>()
                  .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.SeriesId))
                  .ForMember(dest => dest.CustomStartDate, opt => opt.ResolveUsing<NullableDatetimeToString>().FromMember(src => src.CustomStartDate))
                  .ForMember(dest => dest.BacklogSetting, opt => opt.MapFrom(src => (Int32)src.BacklogSetting));
        }

        protected override ILifetimeScope GetApplicationContainer()
        {
            _logger.Info("Starting NzbDrone API");


            var builder = new ContainerBuilder();

            builder.RegisterCoreServices();

            builder.RegisterAssemblyTypes(typeof(Bootstrapper).Assembly)
                   .AsImplementedInterfaces()
                   .SingleInstance();

            builder.RegisterType<ErrorPipeline>().AsSelf().SingleInstance();


            var container = builder.Build();

            ApplicationPipelines.OnError.AddItemToEndOfPipeline(container.Resolve<ErrorPipeline>().HandleException);


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
    }
}