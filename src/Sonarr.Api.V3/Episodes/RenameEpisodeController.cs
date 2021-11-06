using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.MediaFiles;
using Sonarr.Http;

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
        public List<RenameEpisodeResource> GetEpisodes(int seriesId, int? seasonNumber)
        {
            if (seasonNumber.HasValue)
            {
                return _renameEpisodeFileService.GetRenamePreviews(seriesId, seasonNumber.Value).ToResource();
            }

            return _renameEpisodeFileService.GetRenamePreviews(seriesId).ToResource();
        }
    }
}
