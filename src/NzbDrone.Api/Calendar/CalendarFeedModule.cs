using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DDay.iCal;
using NzbDrone.Core.Tv;
using Nancy.Responses;

namespace NzbDrone.Api.Calendar
{
    public class CalendarFeedModule : NancyModule
    {
        private const string ROOT_ROUTE = "/";

        private readonly IEpisodeService _episodeService;

        public CalendarFeedModule(IEpisodeService episodeService)
            : base("feed/calendar")
        {
            _episodeService = episodeService;

            Get["/icalendar.ics"] = options =>
            {
                var resource = GetCalendarFeed();
                return new TextResponse(resource, "text/calendar");
            };
        }

        protected virtual string GetCalendarFeed()
        {
            var start = DateTime.Today.Subtract(TimeSpan.FromDays(7));
            var end = DateTime.Today.AddDays(14);

            var queryStart = Request.Query.Start;
            var queryEnd = Request.Query.End;

            if (queryStart.HasValue) start = DateTime.Parse(queryStart.Value);
            if (queryEnd.HasValue) end = DateTime.Parse(queryEnd.Value);

            // Get the episodes we want in the list.
            var episodes = _episodeService.EpisodesBetweenDates(start, end)
                .Where(v => v.AirDateUtc.HasValue);

            // Create the calendar.
            var icalCalendar = new iCalendar();

            foreach (var series in episodes.GroupBy(v => v.Series))
            {
                /*var icalSeries = icalCalendar.Create<RecurringComponent>();
                icalSeries.UID = series.Key.Id.ToString();
                icalSeries.Name = series.Key.Title;
                icalSeries.Description = string.Format("{0} min on {1}", series.Key.Runtime, series.Key.Network);*/

                foreach (var episode in series)
                {
                    var occurrence = icalCalendar.Create<Event>();
                    occurrence.UID = "NzbDrone_episode_" + episode.Id.ToString();
                    occurrence.Status = episode.HasFile ? EventStatus.Confirmed : EventStatus.Tentative;
                    occurrence.Start = new iCalDateTime(episode.AirDateUtc.Value);
                    occurrence.End = new iCalDateTime(episode.AirDateUtc.Value.AddMinutes(episode.Series.Runtime));
                    //occurrence.... = episode.Overview;
                            
                    switch (episode.Series.SeriesType)
                    {
                        case SeriesTypes.Standard:
                            occurrence.Summary = string.Format("{0} - {1}x{2:00} - {3}", episode.Series.Title, episode.SeasonNumber, episode.EpisodeNumber, episode.Title);
                            break;

                        default:
                            occurrence.Summary = string.Format("{0} - {3}", episode.SeriesTitle, episode.Title);
                            break;
                    }
                }
            }
            
            DDay.iCal.Serialization.ISerializationContext ctx = new DDay.iCal.Serialization.SerializationContext();
            var factory = new DDay.iCal.Serialization.iCalendar.SerializerFactory();
            var serializer = factory.Build(icalCalendar.GetType(), ctx) as DDay.iCal.Serialization.IStringSerializer;

            string output = serializer.SerializeToString(icalCalendar);

            return output;
        }

    }
}
