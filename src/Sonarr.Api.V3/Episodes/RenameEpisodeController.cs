using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.MediaFiles;
using Sonarr.Http;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.Episodes
{
    [V3ApiController("rename")]
    public class RenameEpisodeController : Controller
    {
        private readonly IRenameEpisodeFileService _renameEpisodeFileService;

        public RenameEpisodeController(IRenameEpisodeFileService renameEpisodeFileService)
        {
            _renameEpisodeFileService = renameEpisodeFileService;
        }

        [HttpGet]
        [Produces("application/json")]
        public List<RenameEpisodeResource> GetEpisodes([FromQuery(Name = "seriesId")] List<int> seriesIds, int? seasonNumber)
        {
            if (seriesIds is not { Count: not 0 })
            {
                throw new BadRequestException("seriesIds must be provided");
            }

            if (seasonNumber.HasValue)
            {
                return _renameEpisodeFileService.GetRenamePreviews(seriesIds, seasonNumber.Value).ToResource();
            }

            return _renameEpisodeFileService.GetRenamePreviews(seriesIds).ToResource();
        }
    }
}
