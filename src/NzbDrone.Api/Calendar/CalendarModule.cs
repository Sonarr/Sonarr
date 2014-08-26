using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Api.Episodes;
using NzbDrone.Api.Extensions;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Tv;
using NzbDrone.SignalR;

namespace NzbDrone.Api.Calendar
{
    public class CalendarModule : EpisodeModuleWithSignalR
    {
        private readonly IEpisodeService _episodeService;
        private readonly SeriesRepository _seriesRepository;

        public CalendarModule(IBroadcastSignalRMessage signalRBroadcaster,
                              IEpisodeService episodeService,
                              SeriesRepository seriesRepository)
            : base(episodeService, signalRBroadcaster, "calendar")
        {
            _episodeService = episodeService;
            _seriesRepository = seriesRepository;

            GetResourceAll = GetCalendar;
        }

        private List<EpisodeResource> GetCalendar()
        {
            var start = DateTime.Today;
            var end = DateTime.Today.AddDays(2);

            var queryStart = Request.Query.Start;
            var queryEnd = Request.Query.End;

            if (queryStart.HasValue) start = DateTime.Parse(queryStart.Value);
            if (queryEnd.HasValue) end = DateTime.Parse(queryEnd.Value);

            var resources = ToListResource(() => _episodeService.EpisodesBetweenDates(start, end))
                .LoadSubtype(e => e.SeriesId, _seriesRepository);

            return resources.OrderBy(e => e.AirDateUtc).ToList();
        }
    }
}
