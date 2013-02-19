using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using NzbDrone.Api.QualityProfiles;
using NzbDrone.Core.Repository.Quality;

namespace NzbDrone.Api.Resolvers
{
    public class NextAiringResolver : ValueResolver<Core.Episodes.Series, DateTime?>
    {
        protected override DateTime? ResolveCore(Core.Episodes.Series source)
        {
            if(String.IsNullOrWhiteSpace(source.AirTime) || !source.NextAiring.HasValue)
                return source.NextAiring;

            return source.NextAiring.Value.Add(Convert.ToDateTime(source.AirTime).TimeOfDay)
                                          .AddHours(source.UtcOffset * -1);
        }
    }
}
