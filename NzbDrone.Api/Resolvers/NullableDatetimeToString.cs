using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;

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
