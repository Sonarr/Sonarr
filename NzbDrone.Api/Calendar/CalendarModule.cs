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
        private readonly EpisodeService _episodeService;

        public CalendarModule(EpisodeService episodeService)
            : base("/calendar")
        {
            _episodeService = episodeService;
            Get["/"] = x => GetEpisodesBetweenStartAndEndDate();
        }

        private Response GetEpisodesBetweenStartAndEndDate()
        {
            var start = DateTime.Today;
            var end = DateTime.Today.AddDays(7);

            var queryStart = Request.Query.Start;
            var queryEnd = Request.Query.End;

            if (queryStart.HasValue) start = DateTime.Parse(queryStart.Value);

            if(queryEnd.HasValue) end = DateTime.Parse(queryEnd.Value);

            var episodes = _episodeService.EpisodesBetweenDates(start, end);
            return Mapper.Map<List<Episode>, List<CalendarResource>>(episodes).AsResponse();
        }
    }
}