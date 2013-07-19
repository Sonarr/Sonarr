using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Api.Episodes;
using NzbDrone.Api.Extensions;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.Calendar
{
    public class CalendarModule : NzbDroneRestModule<EpisodeResource>
    {
        private readonly IEpisodeService _episodeService;
        private readonly SeriesRepository _seriesRepository;

        public CalendarModule(IEpisodeService episodeService, SeriesRepository seriesRepository)
            : base("/calendar")
        {
            _episodeService = episodeService;
            _seriesRepository = seriesRepository;

            GetResourceAll = GetPaged;
        }

        private List<EpisodeResource> GetPaged()
        {
            var start = DateTime.Today.AddDays(-1);
            var end = DateTime.Today.AddDays(7);

            var queryStart = Request.Query.Start;
            var queryEnd = Request.Query.End;

            if (queryStart.HasValue) start = DateTime.Parse(queryStart.Value);
            if (queryEnd.HasValue) end = DateTime.Parse(queryEnd.Value);

            var resources = ToListResource(() => _episodeService.EpisodesBetweenDates(start, end))
                .LoadSubtype(e => e.SeriesId, _seriesRepository);

            return resources.OrderBy(e => e.AirDate).ToList();
        }
    }
}
