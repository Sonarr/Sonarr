using System;
using AutoMapper;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Api.Resolvers
{
    public class QualityTypesToIntResolver : ValueResolver<Quality, Int32>
    {
        protected override int ResolveCore(Quality source)
        {
            return source.Id;
        }
    }
}