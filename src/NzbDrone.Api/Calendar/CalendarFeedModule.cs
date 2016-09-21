using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using DDay.iCal;
using NzbDrone.Core.Tv;
using Nancy.Responses;
using NzbDrone.Core.Tags;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Api.Calendar
{
    public class CalendarFeedModule : NzbDroneFeedModule
    {
        private readonly IEpisodeService _episodeService;
        private readonly ITagService _tagService;

        public CalendarFeedModule(IEpisodeService episodeService, ITagService tagService)
            : base("calendar")
        {
            _episodeService = episodeService;
            _tagService = tagService;

            Get["/NzbDrone.ics"] = options => GetCalendarFeed();
        }

        private Response GetCalendarFeed()
        {
            var pastDays = 7;
            var futureDays = 28;
            var start = DateTime.Today.AddDays(-pastDays);
            var end = DateTime.Today.AddDays(futureDays);
            var unmonitored = false;
            var premiersOnly = false;
            var tags = new List<int>();

            // TODO: Remove start/end parameters in v3, they don't work well for iCal
            var queryStart = Request.Query.Start;
            var queryEnd = Request.Query.End;
            var queryPastDays = Request.Query.PastDays;
            var queryFutureDays = Request.Query.FutureDays;
            var queryUnmonitored = Request.Query.Unmonitored;
            var queryPremiersOnly = Request.Query.PremiersOnly;
            var queryTags = Request.Query.Tags;

            if (queryStart.HasValue) start = DateTime.Parse(queryStart.Value);
            if (queryEnd.HasValue) end = DateTime.Parse(queryEnd.Value);

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

            if (queryUnmonitored.HasValue)
            {
                unmonitored = bool.Parse(queryUnmonitored.Value);
            }

            if (queryPremiersOnly.HasValue)
            {
                premiersOnly = bool.Parse(queryPremiersOnly.Value);
            }

            if (queryTags.HasValue)
            {
                var tagInput = (string)queryTags.Value.ToString();
                tags.AddRange(tagInput.Split(',').Select(_tagService.GetTag).Select(t => t.Id));
            }

            var episodes = _episodeService.EpisodesBetweenDates(start, end, unmonitored);
            var icalCalendar = new iCalendar();

            foreach (var episode in episodes.OrderBy(v => v.AirDateUtc.Value))
            {
                if (premiersOnly && (episode.SeasonNumber == 0 || episode.EpisodeNumber != 1))
                {
                    continue;
                }

                if (tags.Any() && tags.None(episode.Series.Tags.Contains))
                {
                    continue;
                }

                var occurrence = icalCalendar.Create<Event>();
                occurrence.UID = "NzbDrone_episode_" + episode.Id.ToString();
                occurrence.Status = episode.HasFile ? EventStatus.Confirmed : EventStatus.Tentative;
                occurrence.Start = new iCalDateTime(episode.AirDateUtc.Value) { HasTime = true };
                occurrence.End = new iCalDateTime(episode.AirDateUtc.Value.AddMinutes(episode.Series.Runtime)) { HasTime = true };
                occurrence.Description = episode.Overview;
                occurrence.Categories = new List<string>() { episode.Series.Network };

                switch (episode.Series.SeriesType)
                {
                    case SeriesTypes.Daily:
                        occurrence.Summary = string.Format("{0} - {1}", episode.Series.Title, episode.Title);
                        break;

                    default:
                        occurrence.Summary = string.Format("{0} - {1}x{2:00} - {3}", episode.Series.Title, episode.SeasonNumber, episode.EpisodeNumber, episode.Title);
                        break;
                }
            }

            var serializer = new DDay.iCal.Serialization.iCalendar.SerializerFactory().Build(icalCalendar.GetType(), new DDay.iCal.Serialization.SerializationContext()) as DDay.iCal.Serialization.IStringSerializer;
            var icalendar = serializer.SerializeToString(icalCalendar);

            return new TextResponse(icalendar, "text/calendar");
        }
    }
}
