using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Nancy;
using NzbDrone.Api.Episodes;
using NzbDrone.Api.Extensions;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.Missing
{
    public class MissingModule : NzbDroneApiModule
    {
        private readonly IEpisodeService _episodeService;

        public MissingModule(IEpisodeService episodeService)
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

            //TODO: Include the Series Title
            return Mapper.Map<List<Episode>, List<EpisodeResource>>(episodes).AsResponse();
        }
    }
}