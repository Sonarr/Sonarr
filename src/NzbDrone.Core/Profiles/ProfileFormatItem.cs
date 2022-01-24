using System.Text.Json.Serialization;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Profiles
{
    public class ProfileFormatItem : IEmbeddedDocument
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int Id { get; set; }
        public CustomFormat Format { get; set; }
        public int Score { get; set; }
    }
}
