using NzbDrone.Core.Datastore;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Profiles.Languages
{
    public class ProfileLanguageItem : IEmbeddedDocument
    {
        public Language Language { get; set; }
        public bool Allowed { get; set; }
    }
}
