using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Blocklisting;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Indexers;
using Sonarr.Http;
using Sonarr.Http.Extensions;
using Sonarr.Http.REST.Attributes;

namespace Sonarr.Api.V5.Blocklist;

[V5ApiController]
public class BlocklistController : Controller
{
    private readonly IBlocklistService _blocklistService;
    private readonly ICustomFormatCalculationService _formatCalculator;

    public BlocklistController(IBlocklistService blocklistService,
                               ICustomFormatCalculationService formatCalculator)
    {
        _blocklistService = blocklistService;
        _formatCalculator = formatCalculator;
    }

    [HttpGet]
    [Produces("application/json")]
    public PagingResource<BlocklistResource> GetBlocklist([FromQuery] PagingRequestResource paging, [FromQuery] int[]? seriesIds = null, [FromQuery] DownloadProtocol[]? protocols = null)
    {
        var pagingResource = new PagingResource<BlocklistResource>(paging);
        var pagingSpec = pagingResource.MapToPagingSpec<BlocklistResource, NzbDrone.Core.Blocklisting.Blocklist>(
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "date",
                "indexer",
                "series.sortTitle",
                "sourceTitle"
            },
            "date",
            SortDirection.Descending);

        if (seriesIds?.Any() == true)
        {
            pagingSpec.FilterExpressions.Add(b => seriesIds.Contains(b.SeriesId));
        }

        if (protocols?.Any() == true)
        {
            pagingSpec.FilterExpressions.Add(b => protocols.Contains(b.Protocol));
        }

        return pagingSpec.ApplyToPage(b => _blocklistService.Paged(pagingSpec), b => BlocklistResourceMapper.MapToResource(b, _formatCalculator));
    }

    [RestDeleteById]
    public ActionResult DeleteBlocklist(int id)
    {
        _blocklistService.Delete(id);

        return NoContent();
    }

    [HttpDelete("bulk")]
    [Produces("application/json")]
    public ActionResult Remove([FromBody] BlocklistBulkResource resource)
    {
        _blocklistService.Delete(resource.Ids);

        return NoContent();
    }
}
