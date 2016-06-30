using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.Putio
{
    public class PutioGenericResponse
    {
        [JsonProperty(PropertyName = "error_message")]
        public string ErrorMessage { get; set; }

        public string Status { get; set; }
    }
}
