using System.Collections.Generic;
using NzbDrone.Api.REST;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Tv;
using NzbDrone.Api.Extensions;
using System.Linq;

namespace NzbDrone.Api.Episodes
{
    public class EpisodeModule : NzbDroneRestModule<EpisodeResource>
    {
        private readonly IEpisodeService _episodeService;
        private readonly MediaFileRepository _mediaFileRepository;

        public EpisodeModule(IEpisodeService episodeService, MediaFileRepository mediaFileRepository)
            : base("/episodes")
        {
            _episodeService = episodeService;
            _mediaFileRepository = mediaFileRepository;

            GetResourceAll = GetEpisodes;
            UpdateResource = SetMonitored;
        }

        private List<EpisodeResource> GetEpisodes()
        {
            var seriesId = (int?)Request.Query.SeriesId;

            if (seriesId == null)
            {
                throw new BadRequestException("seriesId is missing");
            }

            var resource = ToListResource(() => _episodeService.GetEpisodeBySeries(seriesId.Value))
                .LoadSubtype(e => e.EpisodeFileId, _mediaFileRepository);

            return resource.ToList();
        }

        private EpisodeResource SetMonitored(EpisodeResource episodeResource)
        {
            _episodeService.SetEpisodeMonitored(episodeResource.Id, episodeResource.Monitored);

            return episodeResource;
        }
    }
}