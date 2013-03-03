using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Nancy;
using NzbDrone.Api.Extensions;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.Episodes
{
    public class EpisodeModule : NzbDroneApiModule
    {
        private readonly EpisodeService _episodeService;

        public EpisodeModule(EpisodeService episodeService)
            : base("/episodes")
        {
            _episodeService = episodeService;
            Get["/"] = x => GetEpisodesForSeries();
        }

        private Response GetEpisodesForSeries()
        {
            var seriesId = (int)Request.Query.SeriesId;
            var seasonNumber = (int)Request.Query.SeasonNumber;

            var episodes = _episodeService.GetEpisodesBySeason(seriesId, seasonNumber);
            return Mapper.Map<List<Episode>, List<EpisodeResource>>(episodes).AsResponse();
        }
    }
}