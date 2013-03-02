using System;
using System.Collections.Generic;
using System.Globalization;
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
            Get["/{seriesId}"] = x => GetEpisodesForSeries(x.SeriesId);
        }

        private Response GetEpisodesForSeries(int seriesId)
        {
            var episodes = _episodeService.GetEpisodeBySeries(seriesId);
            return Mapper.Map<List<Episode>, List<EpisodeResource>>(episodes).AsResponse();
        }
    }
}