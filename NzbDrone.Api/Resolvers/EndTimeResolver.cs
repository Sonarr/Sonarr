using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using NzbDrone.Api.QualityProfiles;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.Resolvers
{
    public class EndTimeResolver : ValueResolver<Episode, DateTime>
    {
        protected override DateTime ResolveCore(Episode source)
        {
            return source.AirDate.Value.AddMinutes(source.Series.Runtime);
        }
    }
}
