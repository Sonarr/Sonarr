using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Blocklisting;
using NzbDrone.Core.Datastore;
using Sonarr.Http;
using Sonarr.Http.Extensions;
using Sonarr.Http.REST.Attributes;

namespace Sonarr.Api.V3.Blocklist
{
    [V3ApiController]
    public class BlocklistController : Controller
    {
        private readonly IBlocklistService _blocklistService;

        public BlocklistController(IBlocklistService blocklistService)
        {
            _blocklistService = blocklistService;
        }

        [HttpGet]
        public PagingResource<BlocklistResource> GetBlocklist()
        {
            var pagingResource = Request.ReadPagingResourceFromRequest<BlocklistResource>();
            var pagingSpec = pagingResource.MapToPagingSpec<BlocklistResource, NzbDrone.Core.Blocklisting.Blocklist>("date", SortDirection.Descending);

            return pagingSpec.ApplyToPage(_blocklistService.Paged, model => BlocklistResourceMapper.MapToResource(model));
        }

        [RestDeleteById]
        public void DeleteBlocklist(int id)
        {
            _blocklistService.Delete(id);
        }

        [HttpDelete("bulk")]
        public object Remove([FromBody] BlocklistBulkResource resource)
        {
            _blocklistService.Delete(resource.Ids);

            return new { };
        }
    }
}
