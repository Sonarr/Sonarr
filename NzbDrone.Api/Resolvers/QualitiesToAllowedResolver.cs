using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using NzbDrone.Api.Qualities;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Api.Resolvers
{
    public class QualitiesToAllowedResolver : ValueResolver<List<QualityProfileType>, List<Quality>>
    {
        protected override List<Quality> ResolveCore(List<QualityProfileType> source)
        {
            var ids = source.Where(s => s.Allowed).Select(s => s.Id).ToList();

            var qualityTypes = new List<Quality>();

            ids.ForEach(id =>
                            {
                                qualityTypes.Add(Quality.FindById(id));
                            });

            return qualityTypes;
        }
    }
}
