using System;
using AutoMapper;
using NzbDrone.Api.Calendar;
using NzbDrone.Api.QualityProfiles;
using NzbDrone.Api.QualityType;
using NzbDrone.Api.Resolvers;
using NzbDrone.Api.Series;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api
{
    public static class AutomapperBootstraper
    {

        public static void InitializeAutomapper()
        {
            //QualityProfiles
            Mapper.CreateMap<QualityProfile, QualityProfileModel>()
                  .ForMember(dest => dest.Qualities,
                             opt => opt.ResolveUsing<AllowedToQualitiesResolver>().FromMember(src => src.Allowed));

            Mapper.CreateMap<QualityProfileModel, QualityProfile>()
                  .ForMember(dest => dest.Allowed,
                             opt => opt.ResolveUsing<QualitiesToAllowedResolver>().FromMember(src => src.Qualities));

            Mapper.CreateMap<Quality, QualityProfileType>()
                  .ForMember(dest => dest.Allowed, opt => opt.Ignore());

            //QualitySize
            Mapper.CreateMap<QualitySize, QualityTypeModel>()
                  .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.QualityId));

            Mapper.CreateMap<QualityTypeModel, QualitySize>()
                  .ForMember(dest => dest.QualityId, opt => opt.MapFrom(src => src.Id));

            //Series
            Mapper.CreateMap<Core.Tv.Series, SeriesResource>()
                  .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                  .ForMember(dest => dest.CustomStartDate, opt => opt.ResolveUsing<NullableDatetimeToString>().FromMember(src => src.CustomStartDate))
                  .ForMember(dest => dest.BacklogSetting, opt => opt.MapFrom(src => (Int32)src.BacklogSetting))
                  .ForMember(dest => dest.NextAiring, opt => opt.ResolveUsing<NextAiringResolver>())
                  .ForMember(dest => dest.QualityProfileName, opt => opt.MapFrom(src => src.QualityProfile.Name));

            //Calendar
            Mapper.CreateMap<Episode, CalendarResource>()
                  .ForMember(dest => dest.SeriesTitle, opt => opt.MapFrom(src => src.Series.Title))
                  .ForMember(dest => dest.EpisodeTitle, opt => opt.MapFrom(src => src.Title))
                  .ForMember(dest => dest.Start, opt => opt.MapFrom(src => src.AirDate))
                  .ForMember(dest => dest.End, opt => opt.ResolveUsing<EndTimeResolver>());
        }
    }
}