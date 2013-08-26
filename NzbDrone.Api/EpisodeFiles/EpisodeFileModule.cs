using System.Collections.Generic;
using NzbDrone.Api.REST;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Api.Mapping;

namespace NzbDrone.Api.EpisodeFiles
{
    public class EpisodeModule : NzbDroneRestModule<EpisodeFileResource>
    {
        private readonly IMediaFileService _mediaFileService;

        public EpisodeModule(IMediaFileService mediaFileService)
            : base("/episodefile")
        {
            _mediaFileService = mediaFileService;
            GetResourceById = GetEpisodeFile;
            GetResourceAll = GetEpisodeFiles;
            UpdateResource = SetQuality;
        }

        private EpisodeFileResource GetEpisodeFile(int id)
        {
            return _mediaFileService.Get(id).InjectTo<EpisodeFileResource>();
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

        private void SetQuality(EpisodeFileResource episodeFileResource)
        {
            var episodeFile = _mediaFileService.Get(episodeFileResource.Id);
            episodeFile.Quality = episodeFileResource.Quality;
            _mediaFileService.Update(episodeFile);
        }
    }
}