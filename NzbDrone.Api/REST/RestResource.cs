using FluentValidation;
using Newtonsoft.Json;

namespace NzbDrone.Api.REST
{
    public abstract class RestResource
    {
        public int Id { get; set; }

        [JsonIgnore]
        public virtual string ResourceName
        {
            get
            {
                return GetType().Name.ToLower().Replace("resource", "");
            }
        }
    }
}