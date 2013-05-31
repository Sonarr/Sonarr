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
        }

        private List<EpisodeResource> GetEpisodes()
        {
            var seriesId = (int?)Request.Query.SeriesId;
            var seasonNumber = (int?)Request.Query.SeasonNumber;

            if (seriesId == null)
            {
                throw new BadRequestException("seriesId is missing");
            }

            if (seasonNumber == null)
            {
                return ToListResource(() => _episodeService.GetEpisodeBySeries(seriesId.Value));
            }

            return ToListResource(() => _episodeService.GetEpisodesBySeason(seriesId.Value, seasonNumber.Value));
        }
    }
}