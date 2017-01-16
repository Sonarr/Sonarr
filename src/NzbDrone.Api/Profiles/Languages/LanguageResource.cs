using Newtonsoft.Json;
using NzbDrone.Api.REST;

namespace NzbDrone.Api.Profiles.Languages
{
    public class LanguageResource : RestResource
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public new int Id { get; set; }
        public string Name { get; set; }
        public string NameLower => Name.ToLowerInvariant();
    }
}