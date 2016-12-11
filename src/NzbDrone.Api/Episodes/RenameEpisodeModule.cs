using System.Collections.Generic;
using NzbDrone.Api.REST;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Api.Episodes
{
    public class RenameEpisodeModule : NzbDroneRestModule<RenameEpisodeResource>
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
            if (!Request.Query.SeriesId.HasValue)
            {
                throw new BadRequestException("seriesId is missing");
            }

            var seriesId = (int)Request.Query.SeriesId;

            if (Request.Query.SeasonNumber.HasValue)
            {
                var seasonNumber = (int)Request.Query.SeasonNumber;
                return _renameEpisodeFileService.GetRenamePreviews(seriesId, seasonNumber).ToResource();
            }

            return _renameEpisodeFileService.GetRenamePreviews(seriesId).ToResource();
        }
    }
}
