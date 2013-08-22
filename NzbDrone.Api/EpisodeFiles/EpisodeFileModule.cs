using System.Collections.Generic;
using NzbDrone.Api.REST;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Tv;
using NzbDrone.Api.Extensions;
using System.Linq;
using Omu.ValueInjecter;

namespace NzbDrone.Api.EpisodeFiles
{
    public class EpisodeModule : NzbDroneRestModule<EpisodeFileResource>
    {
        private readonly IMediaFileService _mediaFileService;

        public EpisodeModule(IMediaFileService mediaFileService)
            : base("/episodefile")
        {
            _mediaFileService = mediaFileService;
            GetResourceAll = GetEpisodeFiles;
            UpdateResource = SetQuality;
        }

        private List<EpisodeFileResource> GetEpisodeFiles()
        {
            var seriesId = (int?)Request.Query.SeriesId;

            if (seriesId == null)
            {
                throw new BadRequestException("seriesId is missing");
            }

            return ToListResource(() => _mediaFileService.GetFilesBySeries(seriesId.Value));
        }

        private EpisodeFileResource SetQuality(EpisodeFileResource episodeFileResource)
        {
            var episodeFile = _mediaFileService.Get(episodeFileResource.Id);
            episodeFile.Quality = episodeFileResource.Quality;

            _mediaFileService.Update(episodeFile);
            episodeFileResource.InjectFrom(episodeFile);

            return episodeFileResource;
        }
    }
}