using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Profiles
{
    public class ProfileFormatItem : IEmbeddedDocument
    {
        public CustomFormat Format { get; set; }
        public int Score { get; set; }
    }
}
