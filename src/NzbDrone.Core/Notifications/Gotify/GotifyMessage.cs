using Newtonsoft.Json;

namespace NzbDrone.Core.Notifications.Gotify
{
    public class GotifyMessage
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public int Priority { get; set; }
        public GotifyExtras Extras { get; set; }

        public GotifyMessage()
        {
            Extras = new GotifyExtras();
        }

        public void SetContentType(bool isMarkdown)
        {
            var contentType = isMarkdown ? "text/markdown" : "text/plain";

            Extras.ClientDisplay = new GotifyClientDisplay(contentType);
        }
    }

    public class GotifyExtras
    {
        [JsonProperty("client::display")]
        public GotifyClientDisplay ClientDisplay { get; set; }
    }

    public class GotifyClientDisplay
    {
        public string ContentType { get; set; }

        public GotifyClientDisplay(string contentType)
        {
            ContentType = contentType;
        }
    }
}
