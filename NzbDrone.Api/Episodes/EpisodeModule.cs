using System.Collections.Generic;
using NzbDrone.Api.REST;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.Episodes
{
    public class EpisodeModule : NzbDroneRestModule<EpisodeResource>
    {
        private readonly IEpisodeService _episodeService;

        public EpisodeModule(IEpisodeService episodeService)
            : base("/episodes")
        {
            _episodeService = episodeService;

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

            return ToListResource(() => _episodeService.GetEpisodeBySeries(seriesId.Value));
        }

        private void SetMonitored(EpisodeResource episodeResource)
        {
            _episodeService.SetEpisodeMonitored(episodeResource.Id, episodeResource.Monitored);
        }
    }
}