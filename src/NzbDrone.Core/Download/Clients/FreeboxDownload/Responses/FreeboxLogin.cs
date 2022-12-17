using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.FreeboxDownload.Responses
{
    public class FreeboxLogin
    {
        [JsonProperty(PropertyName = "logged_in")]
        public bool LoggedIn { get; set; }
        [JsonProperty(PropertyName = "challenge")]
        public string Challenge { get; set; }
        [JsonProperty(PropertyName = "password_salt")]
        public string PasswordSalt { get; set; }
        [JsonProperty(PropertyName = "password_set")]
        public bool PasswordSet { get; set; }
        [JsonProperty(PropertyName = "session_token")]
        public string SessionToken { get; set; }
    }
}
