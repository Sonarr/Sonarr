using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using ServiceStack.DataAnnotations;

namespace NzbDrone.Core.Qualities
{
    [Alias("QualityProfiles")]
    public class QualityProfile : ModelBase
    {
        public string Name { get; set; }
        public List<Quality> Allowed { get; set; }
        public Quality Cutoff { get; set; }
    }
}