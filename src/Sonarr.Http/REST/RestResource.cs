using System.Text.Json.Serialization;

namespace Sonarr.Http.REST
{
    public abstract class RestResource
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public virtual int Id { get; set; }

        [JsonIgnore]
        public virtual string ResourceName => GetType().Name.ToLowerInvariant().Replace("resource", "");
    }
}
