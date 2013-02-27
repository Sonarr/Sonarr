using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Qualities
{
    public class QualityProfile : ModelBase
    {
        public string Name { get; set; }
        public List<Quality> Allowed { get; set; }
        public Quality Cutoff { get; set; }
    }
}