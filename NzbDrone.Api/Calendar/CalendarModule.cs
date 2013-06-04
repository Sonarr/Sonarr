using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Api.Episodes;
using NzbDrone.Api.Mapping;
using NzbDrone.Common;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.Calendar
{
    public class CalendarModule : NzbDroneRestModule<EpisodeResource>
    {
        private readonly IEpisodeService _episodeService;
        private readonly SeriesService _seriesService;

        public CalendarModule(IEpisodeService episodeService, SeriesService seriesService)
            : base("/calendar")
        {
            _episodeService = episodeService;
            _seriesService = seriesService;

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

            var episodes = _episodeService.EpisodesBetweenDates(start, end);
            var episodeResources = ToListResource(() => episodes);

            var series = _seriesService.GetSeriesInList(episodeResources.SelectDistinct(e => e.SeriesId));
            episodeResources.Join(series, episode => episode.SeriesId, s => s.Id, episode => episode.Series);

            return episodeResources;
        }
    }
}
