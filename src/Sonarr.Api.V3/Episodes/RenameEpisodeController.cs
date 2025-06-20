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
        public List<RenameEpisodeResource> GetEpisodes(int seriesId, int? seasonNumber)
        {
            if (seasonNumber.HasValue)
            {
                return _renameEpisodeFileService.GetRenamePreviews(seriesId, seasonNumber.Value).ToResource();
            }

            return _renameEpisodeFileService.GetRenamePreviews(seriesId).ToResource();
        }

        [HttpGet("multiple")]
        [Produces("application/json")]
        public List<RenameEpisodeResource> GetEpisodes(List<int> seriesIds)
        {
            if (seriesIds is not { Count: not 0 })
            {
                throw new BadRequestException("seriesIds must be provided");
            }

            return _renameEpisodeFileService.GetRenamePreviews(seriesIds).ToResource();
        }
    }
}
