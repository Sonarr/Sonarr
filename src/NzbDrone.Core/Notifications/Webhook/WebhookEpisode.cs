using NzbDrone.Core.Tv;
using System;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookEpisode
    {
        public WebhookEpisode() { }

        public WebhookEpisode(Episode episode)
        {
            Id = episode.Id;
            SeasonNumber = episode.SeasonNumber;
            EpisodeNumber = episode.EpisodeNumber;
            Title = episode.Title;
            AirDate = episode.AirDate;
            AirDateUtc = episode.AirDateUtc;
        }

        public int Id { get; set; }
        public int EpisodeNumber { get; set; }
        public int SeasonNumber { get; set; }
        public string Title { get; set; }
        public string AirDate { get; set; }
        public DateTime? AirDateUtc { get; set; }

        public string Quality { get; set; }
        public int QualityVersion { get; set; }
        public string ReleaseGroup { get; set; }
        public string SceneName { get; set; }
    }
}