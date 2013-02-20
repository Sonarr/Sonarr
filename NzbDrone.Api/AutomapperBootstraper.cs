using System;
using AutoMapper;
using NzbDrone.Api.QualityProfiles;
using NzbDrone.Api.QualityType;
using NzbDrone.Api.Resolvers;
using NzbDrone.Api.Series;
using NzbDrone.Core.Repository.Quality;

namespace NzbDrone.Api
{
    public static class AutomapperBootstraper
    {

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
            Mapper.CreateMap<Core.Tv.Series, SeriesResource>()
                  .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.SeriesId))
                  .ForMember(dest => dest.CustomStartDate, opt => opt.ResolveUsing<NullableDatetimeToString>().FromMember(src => src.CustomStartDate))
                  .ForMember(dest => dest.BacklogSetting, opt => opt.MapFrom(src => (Int32)src.BacklogSetting))
                  .ForMember(dest => dest.NextAiring, opt => opt.ResolveUsing<NextAiringResolver>());
        }
    }
}