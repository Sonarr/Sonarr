using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using NzbDrone.Api.QualityProfiles;
using NzbDrone.Core.Repository.Quality;

namespace NzbDrone.Api.Resolvers
{
    public class QualitiesToAllowedResolver : ValueResolver<List<QualityProfileType>, List<QualityTypes>>
    {
        protected override List<QualityTypes> ResolveCore(List<QualityProfileType> source)
        {
            var ids = source.Where(s => s.Allowed).Select(s => s.Id).ToList();

            var qualityTypes = new List<QualityTypes>();

            ids.ForEach(id =>
                            {
                                qualityTypes.Add(QualityTypes.FindById(id));
                            });

            return qualityTypes;
        }
    }
}
