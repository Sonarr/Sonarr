using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookSeries
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Path { get; set; }
        public int TvdbId { get; set; }
        public int TvMazeId { get; set; }
        public string ImdbId { get; set; }
        public SeriesTypes Type { get; set; }

        public WebhookSeries()
        {
        }

        public WebhookSeries(Series series)
        {
            Id = series.Id;
            Title = series.Title;
            Path = series.Path;
            TvdbId = series.TvdbId;
            TvMazeId = series.TvMazeId;
            ImdbId = series.ImdbId;
            Type = series.SeriesType;
        }
    }
}
