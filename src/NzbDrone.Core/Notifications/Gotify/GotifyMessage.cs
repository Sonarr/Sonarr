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

        public void SetImage(string imageUrl)
        {
            Extras.ClientNotification ??= new GotifyClientNotification();
            Extras.ClientNotification.BigImageUrl = imageUrl;
        }

        public void SetClickUrl(string url)
        {
            Extras.ClientNotification ??= new GotifyClientNotification();
            Extras.ClientNotification.Click = new GotifyClientNotificationClick(url);
        }
    }

    public class GotifyExtras
    {
        [JsonProperty("client::display")]
        public GotifyClientDisplay ClientDisplay { get; set; }

        [JsonProperty("client::notification")]
        public GotifyClientNotification ClientNotification { get; set; }
    }

    public class GotifyClientDisplay
    {
        public string ContentType { get; set; }

        public GotifyClientDisplay(string contentType)
        {
            ContentType = contentType;
        }
    }

    public class GotifyClientNotification
    {
        public string BigImageUrl { get; set; }
        public GotifyClientNotificationClick Click { get; set; }
    }

    public class GotifyClientNotificationClick
    {
        public string Url { get; set; }

        public GotifyClientNotificationClick(string url)
        {
            Url = url;
        }
    }
}
