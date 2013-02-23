using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AutoMapper;
using Nancy;
using NzbDrone.Api.Extentions;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.Calendar
{
    public class CalendarModule : NzbDroneApiModule
    {
        private readonly UpcomingEpisodesProvider _upcomingEpisodesProvider;

        public CalendarModule(UpcomingEpisodesProvider upcomingEpisodesProvider)
            : base("/Calendar")
        {
            _upcomingEpisodesProvider = upcomingEpisodesProvider;
            Get["/"] = x => Calendar();
        }

        private Response Calendar()
        {
            var upcoming = _upcomingEpisodesProvider.UpcomingEpisodes();
            return Mapper.Map<List<Episode>, List<CalendarResource>>(upcoming).AsResponse();
        }
    }
}