using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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