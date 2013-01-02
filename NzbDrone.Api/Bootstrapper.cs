using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using NzbDrone.Api.QualityProfiles;
using NzbDrone.Api.QualityType;
using NzbDrone.Api.Resolvers;
using NzbDrone.Core.Repository.Quality;

namespace NzbDrone.Api
{
    public static class Bootstrapper
    {
        public static void Initialize()
        {
            //QualityProfiles
            Mapper.CreateMap<QualityProfileModel, QualityProfile>()
                  .ForMember(dest => dest.QualityProfileId, opt => opt.MapFrom(src => src.Id))
                  .ForMember(dest => dest.Allowed, opt => opt.ResolveUsing<QualitiesToAllowedResolver>().FromMember(src => src.Qualities));

            Mapper.CreateMap<QualityProfile, QualityProfileModel>()
                  .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.QualityProfileId))
                  .ForMember(dest => dest.Qualities, opt => opt.ResolveUsing<AllowedToQualitiesResolver>().FromMember(src => src.Allowed));

            Mapper.CreateMap<QualityTypes, QualityProfileType>()
                  .ForMember(dest => dest.Allowed, opt => opt.Ignore());

            //QualityTypes
            Mapper.CreateMap<QualityTypeModel, Core.Repository.Quality.QualityType>()
                  .ForMember(dest => dest.QualityTypeId, opt => opt.MapFrom(src => src.Id));

            Mapper.CreateMap<Core.Repository.Quality.QualityType, QualityTypeModel>()
                  .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.QualityTypeId));
        }
    }
}
