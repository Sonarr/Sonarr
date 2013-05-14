using NzbDrone.Api.Episodes;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.Missing
{
    public class MissingModule : NzbDroneRestModule<EpisodeResource>
    {
        private readonly IEpisodeService _episodeService;

        public MissingModule(IEpisodeService episodeService)
            :base("missing")
        {
            _episodeService = episodeService;
            GetResourcePaged = GetMissingEpisodes;
        }

        private PagingResource<EpisodeResource> GetMissingEpisodes(PagingResource<EpisodeResource> pagingResource)
        {
            var pagingSpec = new PagingSpec<Episode>
            {
                Page = pagingResource.Page,
                PageSize = pagingResource.PageSize,
                SortKey = pagingResource.SortKey,
                SortDirection = pagingResource.SortDirection
            };

            return ApplyToPage(_episodeService.EpisodesWithoutFiles, pagingSpec);
        }
    }
}