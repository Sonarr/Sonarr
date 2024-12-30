using Workarr.CustomFormats;
using Workarr.Datastore;

namespace Workarr.Profiles
{
    public class ProfileFormatItem : IEmbeddedDocument
    {
        public CustomFormat Format { get; set; }
        public int Score { get; set; }
    }
}
