using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tags;
using NzbDrone.Core.Tv;
using Sonarr.Http;

namespace Sonarr.Api.V5.Calendar;

[V5FeedController("calendar")]
public class CalendarFeedController : Controller
{
    private readonly IEpisodeService _episodeService;
    private readonly ISeriesService _seriesService;
    private readonly ITagService _tagService;

    public CalendarFeedController(IEpisodeService episodeService, ISeriesService seriesService, ITagService tagService)
    {
        _episodeService = episodeService;
        _seriesService = seriesService;
        _tagService = tagService;
    }

    [HttpGet("Sonarr.ics")]
    public IActionResult GetCalendarFeed(int pastDays = 7, int futureDays = 28, string tags = "", bool unmonitored = false, bool premieresOnly = false, bool asAllDay = false, bool includeSpecials = true)
    {
        var start = DateTime.Today.AddDays(-pastDays);
        var end = DateTime.Today.AddDays(futureDays);
        var parsedTags = new List<int>();

        if (tags.IsNotNullOrWhiteSpace())
        {
            parsedTags.AddRange(tags.Split(',').Select(_tagService.GetTag).Select(t => t.Id));
        }

        var episodes = _episodeService.EpisodesBetweenDates(start, end, unmonitored, includeSpecials);
        var allSeries = _seriesService.GetAllSeries();
        var calendar = new Ical.Net.Calendar
        {
            ProductId = "-//sonarr.tv//Sonarr//EN"
        };

        var calendarName = "Sonarr TV Schedule";
        calendar.AddProperty(new CalendarProperty("NAME", calendarName));
        calendar.AddProperty(new CalendarProperty("X-WR-CALNAME", calendarName));

        foreach (var episode in episodes.OrderBy(v => v.AirDateUtc!.Value))
        {
            var series = allSeries.SingleOrDefault(s => s.Id == episode.SeriesId);

            if (series == null)
            {
                continue;
            }

            if (premieresOnly && (episode.SeasonNumber == 0 || episode.EpisodeNumber != 1))
            {
                continue;
            }

            if (parsedTags.Any() && parsedTags.None(series.Tags.Contains))
            {
                continue;
            }

            var occurrence = calendar.Create<CalendarEvent>();
            occurrence.Uid = "NzbDrone_episode_" + episode.Id;
            occurrence.Status = episode.HasFile ? EventStatus.Confirmed : EventStatus.Tentative;
            occurrence.Description = episode.Overview;
            occurrence.Categories = new List<string>() { series.Network };

            if (asAllDay)
            {
                occurrence.Start = new CalDateTime(episode.AirDateUtc!.Value.ToLocalTime()) { HasTime = false };
            }
            else
            {
                occurrence.Start = new CalDateTime(episode.AirDateUtc!.Value) { HasTime = true };
                occurrence.End = new CalDateTime(episode.AirDateUtc.Value.AddMinutes(series.Runtime)) { HasTime = true };
            }

            switch (series.SeriesType)
            {
                case SeriesTypes.Daily:
                    occurrence.Summary = $"{series.Title} - {episode.Title}";
                    break;
                default:
                    occurrence.Summary = $"{series.Title} - {episode.SeasonNumber}x{episode.EpisodeNumber:00} - {episode.Title}";
                    break;
            }
        }

        var serializer = (IStringSerializer)new SerializerFactory().Build(calendar.GetType(), new SerializationContext());
        var icalendar = serializer.SerializeToString(calendar);

        return Content(icalendar, "text/calendar");
    }
}
