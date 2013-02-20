using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using NzbDrone.Api.QualityProfiles;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.Resolvers
{
    public class AirTimeResolver : ValueResolver<Episode, DateTime?>
    {
        protected override DateTime? ResolveCore(Episode source)
        {
            if(String.IsNullOrWhiteSpace(source.Series.AirTime) || !source.AirDate.HasValue)
                return source.AirDate;

            return source.AirDate.Value.Add(Convert.ToDateTime(source.Series.AirTime).TimeOfDay)
                                          .AddHours(source.Series.UtcOffset * -1);
        }
    }
}
