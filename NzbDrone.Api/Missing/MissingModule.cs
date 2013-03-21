using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Nancy;
using NzbDrone.Api.Extensions;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.Missing
{
    public class MissingModule : NzbDroneApiModule
    {
        private readonly EpisodeService _episodeService;

        public MissingModule(EpisodeService episodeService)
            : base("/missing")
        {
            _episodeService = episodeService;
            Get["/"] = x => GetMissingEpisodes();
        }

        private Response GetMissingEpisodes()
        {
            bool includeSpecials;
            Boolean.TryParse(PrimitiveExtensions.ToNullSafeString(Request.Query.IncludeSpecials), out includeSpecials);

            var episodes = _episodeService.EpisodesWithoutFiles(includeSpecials);
            return Mapper.Map<List<Episode>, List<MissingResource>>(episodes).AsResponse();
        }
    }
}