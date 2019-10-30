using NzbDrone.Core.Blacklisting;
using NzbDrone.Core.Datastore;
using Sonarr.Http;

namespace Sonarr.Api.V3.Blacklist
{
    public class BlacklistModule : SonarrRestModule<BlacklistResource>
    {
        private readonly IBlacklistService _blacklistService;

        public BlacklistModule(IBlacklistService blacklistService)
        {
            _blacklistService = blacklistService;
            GetResourcePaged = GetBlacklist;
            DeleteResource = DeleteBlacklist;
        }

        private PagingResource<BlacklistResource> GetBlacklist(PagingResource<BlacklistResource> pagingResource)
        {
            var pagingSpec = pagingResource.MapToPagingSpec<BlacklistResource, NzbDrone.Core.Blacklisting.Blacklist>("date", SortDirection.Descending);

            return ApplyToPage(_blacklistService.Paged, pagingSpec, BlacklistResourceMapper.MapToResource);
        }

        private void DeleteBlacklist(int id)
        {
            _blacklistService.Delete(id);
        }
    }
}