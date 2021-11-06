using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tags;
using NzbDrone.Core.Tv;
using Sonarr.Http;

namespace Sonarr.Api.V3.Calendar
{
    [V3FeedController("calendar")]
    public class CalendarFeedController : Controller
    {
        private readonly IEpisodeService _episodeService;
        private readonly ITagService _tagService;

        public CalendarFeedController(IEpisodeService episodeService, ITagService tagService)
        {
            _episodeService = episodeService;
            _tagService = tagService;
        }

        [HttpGet("Sonarr.ics")]
        public IActionResult GetCalendarFeed(int pastDays = 7, int futureDays = 28, string tagList = "", bool unmonitored = false, bool premieresOnly = false, bool asAllDay = false)
        {
            var start = DateTime.Today.AddDays(-pastDays);
            var end = DateTime.Today.AddDays(futureDays);
            var tags = new List<int>();

            if (tagList.IsNotNullOrWhiteSpace())
            {
                tags.AddRange(tagList.Split(',').Select(_tagService.GetTag).Select(t => t.Id));
            }

            var episodes = _episodeService.EpisodesBetweenDates(start, end, unmonitored);
            var calendar = new Ical.Net.Calendar
            {
                ProductId = "-//sonarr.tv//Sonarr//EN"
            };

            var calendarName = "Sonarr TV Schedule";
            calendar.AddProperty(new CalendarProperty("NAME", calendarName));
            calendar.AddProperty(new CalendarProperty("X-WR-CALNAME", calendarName));

            foreach (var episode in episodes.OrderBy(v => v.AirDateUtc.Value))
            {
                if (premieresOnly && (episode.SeasonNumber == 0 || episode.EpisodeNumber != 1))
                {
                    continue;
                }

                if (tags.Any() && tags.None(episode.Series.Tags.Contains))
                {
                    continue;
                }

                var occurrence = calendar.Create<CalendarEvent>();
                occurrence.Uid = "NzbDrone_episode_" + episode.Id;
                occurrence.Status = episode.HasFile ? EventStatus.Confirmed : EventStatus.Tentative;
                occurrence.Description = episode.Overview;
                occurrence.Categories = new List<string>() { episode.Series.Network };

                if (asAllDay)
                {
                    occurrence.Start = new CalDateTime(episode.AirDateUtc.Value.ToLocalTime()) { HasTime = false };
                }
                else
                {
                    occurrence.Start = new CalDateTime(episode.AirDateUtc.Value) { HasTime = true };
                    occurrence.End = new CalDateTime(episode.AirDateUtc.Value.AddMinutes(episode.Series.Runtime)) { HasTime = true };
                }

                switch (episode.Series.SeriesType)
                {
                    case SeriesTypes.Daily:
                        occurrence.Summary = $"{episode.Series.Title} - {episode.Title}";
                        break;
                    default:
                        occurrence.Summary = $"{episode.Series.Title} - {episode.SeasonNumber}x{episode.EpisodeNumber:00} - {episode.Title}";
                        break;
                }
            }

            var serializer = (IStringSerializer)new SerializerFactory().Build(calendar.GetType(), new SerializationContext());
            var icalendar = serializer.SerializeToString(calendar);

            return Content(icalendar, "text/calendar");
        }
    }
}
