using System.Collections.Generic;
using NzbDrone.Core.MediaFiles;
using Sonarr.Http;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.Episodes
{
    public class RenameEpisodeModule : SonarrRestModule<RenameEpisodeResource>
    {
        private readonly IRenameEpisodeFileService _renameEpisodeFileService;

        public RenameEpisodeModule(IRenameEpisodeFileService renameEpisodeFileService)
            : base("rename")
        {
            _renameEpisodeFileService = renameEpisodeFileService;

            GetResourceAll = GetEpisodes;
        }

        private List<RenameEpisodeResource> GetEpisodes()
        {
            int seriesId;

            if (Request.Query.SeriesId.HasValue)
            {
                seriesId = (int)Request.Query.SeriesId;
            }
            else
            {
                throw new BadRequestException("seriesId is missing");
            }

            if (Request.Query.SeasonNumber.HasValue)
            {
                var seasonNumber = (int)Request.Query.SeasonNumber;
                return _renameEpisodeFileService.GetRenamePreviews(seriesId, seasonNumber).ToResource();
            }

            return _renameEpisodeFileService.GetRenamePreviews(seriesId).ToResource();
        }
    }
}
