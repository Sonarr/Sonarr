using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using NzbDrone.Core.Repository.Quality;

namespace NzbDrone.Api.Resolvers
{
    public class QualityTypesToIntResolver : ValueResolver<QualityTypes, Int32>
    {
        protected override int ResolveCore(QualityTypes source)
        {
            return source.Id;
        }
    }
}