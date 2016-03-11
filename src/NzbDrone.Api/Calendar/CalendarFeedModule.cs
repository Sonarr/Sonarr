using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using DDay.iCal;
using NzbDrone.Core.Tv;
using Nancy.Responses;

namespace NzbDrone.Api.Calendar
{
    public class CalendarFeedModule : NzbDroneFeedModule
    {
        private readonly IEpisodeService _episodeService;

        public CalendarFeedModule(IEpisodeService episodeService)
            : base("calendar")
        {
            _episodeService = episodeService;

            Get["/NzbDrone.ics"] = options => GetCalendarFeed();
        }

        private Response GetCalendarFeed()
        {
            var pastDays = 7;
            var futureDays = 28;            
            var start = DateTime.Today.AddDays(-pastDays);
            var end = DateTime.Today.AddDays(futureDays);

            // TODO: Remove start/end parameters in v3, they don't work well for iCal
            var queryStart = Request.Query.Start;
            var queryEnd = Request.Query.End;
            var queryPastDays = Request.Query.PastDays;
            var queryFutureDays = Request.Query.FutureDays;

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

            var episodes = _episodeService.EpisodesBetweenDates(start, end, false);
            var icalCalendar = new iCalendar();

            foreach (var episode in episodes.OrderBy(v => v.AirDateUtc.Value))
            {
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
