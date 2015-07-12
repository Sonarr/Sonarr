using NzbDrone.Core.Datastore;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Profiles.Qualities
{
    public class ProfileQualityItem : IEmbeddedDocument
    {
        public Quality Quality { get; set; }
        public bool Allowed { get; set; }
    }
}
