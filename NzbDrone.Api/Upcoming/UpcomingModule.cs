using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AutoMapper;
using FluentValidation;
using Nancy;
using NzbDrone.Api.Extentions;
using NzbDrone.Api.Series;
using NzbDrone.Common;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.Upcoming
{
    public class UpcomingModule : NzbDroneApiModule
    {
        private readonly UpcomingEpisodesProvider _upcomingEpisodesProvider;

        public UpcomingModule(UpcomingEpisodesProvider upcomingEpisodesProvider)
            : base("/Upcoming")
        {
            _upcomingEpisodesProvider = upcomingEpisodesProvider;
            Get["/"] = x => Upcoming();
        }

        private Response Upcoming()
        {
            var upcoming = _upcomingEpisodesProvider.UpcomingEpisodes();
            return Mapper.Map<List<Episode>, List<UpcomingResource>>(upcoming).AsResponse();
        }
    }
}