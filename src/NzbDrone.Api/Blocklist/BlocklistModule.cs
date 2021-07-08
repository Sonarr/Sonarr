using NzbDrone.Core.Blocklisting;
using NzbDrone.Core.Datastore;
using Sonarr.Http;

namespace NzbDrone.Api.Blocklist
{
    public class BlocklistModule : SonarrRestModule<BlocklistResource>
    {
        private readonly BlocklistService _blocklistService;

        public BlocklistModule(BlocklistService blocklistService)
        {
            _blocklistService = blocklistService;
            GetResourcePaged = Blocklist;
            DeleteResource = DeleteBlockList;
        }

        private PagingResource<BlocklistResource> Blocklist(PagingResource<BlocklistResource> pagingResource)
        {
            var pagingSpec = pagingResource.MapToPagingSpec<BlocklistResource, Core.Blocklisting.Blocklist>("id", SortDirection.Ascending);

            return ApplyToPage(_blocklistService.Paged, pagingSpec, BlocklistResourceMapper.MapToResource);
        }

        private void DeleteBlockList(int id)
        {
            _blocklistService.Delete(id);
        }
    }
}
