using System.Collections.Generic;
using NzbDrone.Core.Qualities;
using NzbDrone.Api.Mapping;
using System.Linq;

namespace NzbDrone.Api.Qualities
{
    public class QualityProfileSchemaModule : NzbDroneRestModule<QualityProfileResource>
    {
        public QualityProfileSchemaModule()
            : base("/qualityprofiles/schema")
        {
            GetResourceAll = GetAll;
        }

        private List<QualityProfileResource> GetAll()
        {
            var profile = new QualityProfile();
            profile.Cutoff = Quality.Unknown;
            profile.Allowed = new List<Quality>();

            return new List<QualityProfileResource>{ QualityToResource(profile)};
        }

        private static QualityProfileResource QualityToResource(QualityProfile profile)
        {
            return new QualityProfileResource
                {
                    Available = Quality.All()
                        .Where(c => !profile.Allowed.Any(q => c.Id == q.Id))
                        .InjectTo<List<QualityResource>>(),

                    Allowed = profile.Allowed.InjectTo<List<QualityResource>>(),
                    Name = profile.Name,
                    Id = profile.Id
                };
        }
    }
}