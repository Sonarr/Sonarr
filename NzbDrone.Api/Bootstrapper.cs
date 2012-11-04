using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using NzbDrone.Api.QualityProfiles;
using NzbDrone.Core.Repository.Quality;

namespace NzbDrone.Api
{
    public static class Bootstrapper
    {
        public static void Initialize()
        {
            Mapper.CreateMap<QualityProfile, QualityProfileModel>()
                  .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.QualityProfileId));

            Mapper.CreateMap<QualityProfileModel, QualityProfile>()
                  .ForMember(dest => dest.QualityProfileId, opt => opt.MapFrom(src => src.Id));
        }
    }
}
