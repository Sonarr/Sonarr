using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using NzbDrone.Api.Qualities;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Api.Resolvers
{
    public class AllowedToQualitiesResolver : ValueResolver<List<Quality>, List<QualityProfileType>>
    {
        protected override List<QualityProfileType> ResolveCore(List<Quality> source)
        {
            var qualities = Mapper.Map<List<Quality>, List<QualityProfileType>>(Quality.All().Where(q => q.Id > 0).ToList());

            qualities.ForEach(quality =>
            {
                quality.Allowed = source.SingleOrDefault(q => q.Id == quality.Id) != null;
            });

            return qualities;
        }
    }
}
