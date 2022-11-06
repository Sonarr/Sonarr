using System;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookEpisode
    {
        public WebhookEpisode()
        {
        }

        public WebhookEpisode(Episode episode)
        {
            Id = episode.Id;
            SeasonNumber = episode.SeasonNumber;
            EpisodeNumber = episode.EpisodeNumber;
            Title = episode.Title;
            Overview = episode.Overview;
            AirDate = episode.AirDate;
            AirDateUtc = episode.AirDateUtc;
            SeriesId = episode.SeriesId;
        }

        public int Id { get; set; }
        public int EpisodeNumber { get; set; }
        public int SeasonNumber { get; set; }
        public string Title { get; set; }
        public string Overview { get; set; }
        public string AirDate { get; set; }
        public DateTime? AirDateUtc { get; set; }
        public int SeriesId { get; set; }
    }
}
