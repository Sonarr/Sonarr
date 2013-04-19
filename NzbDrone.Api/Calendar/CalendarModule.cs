using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AutoMapper;
using Nancy;
using NzbDrone.Api.Extensions;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.Calendar
{
    public class CalendarModule : NzbDroneApiModule
    {
        private readonly IEpisodeService _episodeService;

        public CalendarModule(IEpisodeService episodeService)
            : base("/calendar")
        {
            _episodeService = episodeService;
            Get["/"] = x => GetEpisodesBetweenStartAndEndDate();
        }

        private Response GetEpisodesBetweenStartAndEndDate()
        {
            var start = DateTime.Today.AddDays(-1);
            var end = DateTime.Today.AddDays(7);

            var queryStart = Request.Query.Start;
            var queryEnd = Request.Query.End;

            if (queryStart.HasValue) start = DateTime.Parse(queryStart.Value);

            if(queryEnd.HasValue) end = DateTime.Parse(queryEnd.Value);

            var episodes = _episodeService.EpisodesBetweenDates(start, end);

            //Todo: This should be done on episode data refresh - because it can be used in multiple places
            var groups = episodes.GroupBy(e => new { e.SeriesId, e.AirDate }).Where(g => g.Count() > 1).ToList();

            foreach (var group in groups)
            {
                //Order by Episode Number
                int episodeCount = 0;
                foreach (var episode in group.OrderBy(e => e.SeasonNumber).ThenBy(e => e.EpisodeNumber))
                {
                    episode.AirDate = episode.AirDate.Value.AddMinutes(episode.Series.Runtime * episodeCount);
                    episodeCount++;
                }
            }

            return Mapper.Map<List<Episode>, List<CalendarResource>>(episodes).AsResponse();
        }
    }
}