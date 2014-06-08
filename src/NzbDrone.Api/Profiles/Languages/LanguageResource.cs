using System;
using Newtonsoft.Json;
using NzbDrone.Api.REST;

namespace NzbDrone.Api.Profiles.Languages
{
    public class LanguageResource : RestResource
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public Int32 Id { get; set; }
        public String Name { get; set; }
        public String NameLower { get { return Name.ToLowerInvariant(); } }
    }
}