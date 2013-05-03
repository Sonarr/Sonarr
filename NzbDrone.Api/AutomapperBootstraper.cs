using System;
using AutoMapper;
using NzbDrone.Api.Calendar;
using NzbDrone.Api.Episodes;
using NzbDrone.Api.History;
using NzbDrone.Api.Missing;
using NzbDrone.Api.QualityProfiles;
using NzbDrone.Api.QualityType;
using NzbDrone.Api.Resolvers;
using NzbDrone.Api.Series;
using NzbDrone.Core.Datastore;
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
            Mapper.CreateMap<QualitySize, QualitySizeResource>()
                  .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.QualityId));

            Mapper.CreateMap<QualitySizeResource, QualitySize>()
                  .ForMember(dest => dest.QualityId, opt => opt.MapFrom(src => src.Id));

            //Episode
            Mapper.CreateMap<Episode, EpisodeResource>();

            //Episode Paging
            Mapper.CreateMap<PagingSpec<Episode>, PagingResource<EpisodeResource>>();

            //History
            Mapper.CreateMap<Core.History.History, HistoryResource>();
            Mapper.CreateMap<PagingSpec<Core.History.History>, PagingResource<HistoryResource>>();
        }
    }
}