using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using NzbDrone.Api.QualityProfiles;
using NzbDrone.Core.Repository.Quality;

namespace NzbDrone.Api.Resolvers
{
    public class NullableDatetimeToString : ValueResolver<DateTime?, String>
    {
        protected override String ResolveCore(DateTime? source)
        {
            if(!source.HasValue)
                return String.Empty;

            return source.Value.ToString("yyyy-MM-dd");
        }
    }
}
