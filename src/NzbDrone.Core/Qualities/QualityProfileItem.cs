using System.Collections.Generic;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Qualities
{
    public class QualityProfileItem : IEmbeddedDocument
    {
        public Quality Quality { get; set; }
        public bool Allowed { get; set; }
    }
}
