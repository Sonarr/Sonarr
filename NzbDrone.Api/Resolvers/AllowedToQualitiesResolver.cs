using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using NzbDrone.Api.QualityProfiles;
using NzbDrone.Core.Repository.Quality;

namespace NzbDrone.Api.Resolvers
{
    public class AllowedToQualitiesResolver : ValueResolver<List<QualityTypes>, List<QualityProfileType>>
    {
        protected override List<QualityProfileType> ResolveCore(List<QualityTypes> source)
        {
            var qualities = Mapper.Map<List<QualityTypes>, List<QualityProfileType>>(QualityTypes.All().Where(q => q.Id > 0).ToList());

            qualities.ForEach(quality =>
            {
                quality.Allowed = source.SingleOrDefault(q => q.Id == quality.Id) != null;
            });

            return qualities;
        }
    }
}
