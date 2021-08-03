using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Nancy;
using Nancy.Responses;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tags;
using NzbDrone.Core.Tv;
using Sonarr.Http.Extensions;

namespace Sonarr.Api.V3.Calendar
{
    public class CalendarFeedModule : SonarrV3FeedModule
    {
        private readonly IEpisodeService _episodeService;
        private readonly ITagService _tagService;

        public CalendarFeedModule(IEpisodeService episodeService, ITagService tagService)
            : base("calendar")
        {
            _episodeService = episodeService;
            _tagService = tagService;

            Get("/Sonarr.ics",  options => GetCalendarFeed());
        }

        private object GetCalendarFeed()
        {
            var pastDays = 7;
            var futureDays = 28;
            var start = DateTime.Today.AddDays(-pastDays);
            var end = DateTime.Today.AddDays(futureDays);
            var unmonitored = Request.GetBooleanQueryParameter("unmonitored");

            // There was a typo, recognize both the correct 'premieresOnly' and mistyped 'premiersOnly' boolean for background compat.
            var premieresOnly = Request.GetBooleanQueryParameter("premieresOnly") || Request.GetBooleanQueryParameter("premiersOnly");
            var asAllDay = Request.GetBooleanQueryParameter("asAllDay");
            var tags = new List<int>();

            var queryPastDays = Request.Query.PastDays;
            var queryFutureDays = Request.Query.FutureDays;
            var queryTags = Request.Query.Tags;

            if (queryPastDays.HasValue)
            {
                pastDays = int.Parse(queryPastDays.Value);
                start = DateTime.Today.AddDays(-pastDays);
            }

            if (queryFutureDays.HasValue)
            {
                futureDays = int.Parse(queryFutureDays.Value);
                end = DateTime.Today.AddDays(futureDays);
            }

            if (queryTags.HasValue)
            {
                var tagInput = (string)queryTags.Value.ToString();
                tags.AddRange(tagInput.Split(',').Select(_tagService.GetTag).Select(t => t.Id));
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

            return new TextResponse(icalendar, "text/calendar");
        }
    }
}
