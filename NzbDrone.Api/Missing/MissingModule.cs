using NzbDrone.Core.Datastore;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.Missing
{
    public class MissingModule : NzbDroneRestModule<MissingResource>
    {
        private readonly IEpisodeService _episodeService;

        public MissingModule(IEpisodeService episodeService)
        {
            _episodeService = episodeService;
            GetResourcePaged = GetMissingEpisodes;
        }

        private PagingResource<MissingResource> GetMissingEpisodes(PagingResource<MissingResource> pagingResource)
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