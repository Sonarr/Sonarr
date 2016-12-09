using Newtonsoft.Json;

namespace NzbDrone.Api.REST
{
    public abstract class RestResource
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Id { get; set; }

        [JsonIgnore]
        public virtual string ResourceName => GetType().Name.ToLowerInvariant().Replace("resource", "");
    }
}