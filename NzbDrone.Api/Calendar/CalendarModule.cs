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
            Get["/"] = x => Calendar();
        }

        private Response Calendar()
        {
            var year = DateTime.Now.Year;
            //Todo: This is just for testing
            //var month = DateTime.Now.Month;
            var month = 1;

            var yearQuery = Request.Query.Year;
            var monthQuery = Request.Query.Month;

            if (yearQuery.HasValue) year = Convert.ToInt32(yearQuery.Value);

            if(monthQuery.HasValue) month = Convert.ToInt32(monthQuery.Value);

            var episodes = _episodeService.GetEpisodesAiredInMonth(year, month);
            return Mapper.Map<List<Episode>, List<CalendarResource>>(episodes).AsResponse();
        }
    }
}