using System.Collections.Generic;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Qualities
{
    public class QualityProfile : ModelBase
    {
        public string Name { get; set; }
        public Quality Cutoff { get; set; }
        public List<QualityProfileItem> Items { get; set; }
    }
}