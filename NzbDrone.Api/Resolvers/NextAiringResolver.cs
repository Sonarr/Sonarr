using System;
using System.Linq;
using AutoMapper;

namespace NzbDrone.Api.Resolvers
{
    public class NextAiringResolver : ValueResolver<Core.Tv.Series, DateTime?>
    {
        protected override DateTime? ResolveCore(Core.Tv.Series source)
        {
            if (String.IsNullOrWhiteSpace(source.AirTime) || !source.NextAiring.HasValue)
                return source.NextAiring;

            return source.NextAiring.Value.Add(Convert.ToDateTime(source.AirTime).TimeOfDay)
                                          .AddHours(source.UtcOffset * -1);
        }
    }
}
