using System;
using Newtonsoft.Json;
namespace NzbDrone.Core.Notifications.Trakt.Resource
{
    public class TraktEpisodeResource
    {
        public int Number { get; set; }

        [JsonProperty(PropertyName = "collected_at")]
        public DateTime CollectedAt { get; set; }
        public string Resolution { get; set; }

        [JsonProperty(PropertyName = "audio_channels")]
        public string AudioChannels { get; set; }
        public string Audio { get; set; }

        [JsonProperty(PropertyName = "media_type")]
        public string MediaType { get; set; }
    }
}
