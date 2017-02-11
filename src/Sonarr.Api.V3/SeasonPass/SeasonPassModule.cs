using System.Linq;
using Nancy;
using NzbDrone.Core.Tv;
using Sonarr.Http.Extensions;

namespace Sonarr.Api.V3.SeasonPass
{
    public class SeasonPassModule : SonarrV3Module
    {
        private readonly ISeriesService _seriesService;
        private readonly IEpisodeMonitoredService _episodeMonitoredService;

        public SeasonPassModule(ISeriesService seriesService, IEpisodeMonitoredService episodeMonitoredService)
            : base("/seasonpass")
        {
            _seriesService = seriesService;
            _episodeMonitoredService = episodeMonitoredService;
            Post["/"] = series => UpdateAll();
        }

        private Response UpdateAll()
        {
            //Read from request
            var request = Request.Body.FromJson<SeasonPassResource>();
            var seriesToUpdate = _seriesService.GetSeries(request.Series.Select(s => s.Id));

            foreach (var s in request.Series)
            {
                var series = seriesToUpdate.Single(c => c.Id == s.Id);

                if (s.Monitored.HasValue)
                {
                    series.Monitored = s.Monitored.Value;
                }

                if (s.Seasons != null && s.Seasons.Any())
                {
                    foreach (var seriesSeason in series.Seasons)
                    {
                        var season = s.Seasons.FirstOrDefault(c => c.SeasonNumber == seriesSeason.SeasonNumber);

                        if (season != null)
                        {
                            seriesSeason.Monitored = season.Monitored;
                        }
                    }
                }

                _episodeMonitoredService.SetEpisodeMonitoredStatus(series, request.MonitoringOptions);
            }

            return "ok".AsResponse(HttpStatusCode.Accepted);
        }
    }
}
