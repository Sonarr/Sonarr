using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookSeries
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string TitleSlug { get; set; }
        public string Path { get; set; }
        public int TvdbId { get; set; }
        public int TvMazeId { get; set; }
        public int TmdbId { get; set; }
        public string ImdbId { get; set; }
        public SeriesTypes Type { get; set; }
        public int Year { get; set; }
        public List<string> Genres { get; set; }
        public List<WebhookImage> Images { get; set; }
        public List<string> Tags { get; set; }

        public WebhookSeries()
        {
        }

        public WebhookSeries(Series series, List<string> tags)
        {
            Id = series.Id;
            Title = series.Title;
            TitleSlug = series.TitleSlug;
            Path = series.Path;
            TvdbId = series.TvdbId;
            TvMazeId = series.TvMazeId;
            TmdbId = series.TmdbId;
            ImdbId = series.ImdbId;
            Type = series.SeriesType;
            Year = series.Year;
            Genres = series.Genres;
            Images = series.Images.Select(i => new WebhookImage(i)).ToList();
            Tags = tags;
        }
    }
}
