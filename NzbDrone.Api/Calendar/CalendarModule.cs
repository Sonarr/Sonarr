using System;
using System.Collections.Generic;
using AutoMapper;
using Nancy;
using NzbDrone.Api.Episodes;
using NzbDrone.Api.Extensions;
using NzbDrone.Api.Mapping;
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
            return episodes.InjectTo<List<EpisodeResource>>().AsResponse();
        }
    }
}