using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Blocklisting;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Indexers;
using Sonarr.Http;
using Sonarr.Http.Extensions;
using Sonarr.Http.REST.Attributes;

namespace Sonarr.Api.V3.Blocklist
{
    [V3ApiController]
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
        public PagingResource<BlocklistResource> GetBlocklist([FromQuery] PagingRequestResource paging, [FromQuery] int[] seriesIds = null, DownloadProtocol? protocol = null)
        {
            var pagingResource = new PagingResource<BlocklistResource>(paging);
            var pagingSpec = pagingResource.MapToPagingSpec<BlocklistResource, NzbDrone.Core.Blocklisting.Blocklist>("date", SortDirection.Descending);

            return pagingSpec.ApplyToPage(spec => GetBlocklist(spec, seriesIds?.ToHashSet(), protocol), model => BlocklistResourceMapper.MapToResource(model, _formatCalculator));
        }

        private PagingSpec<NzbDrone.Core.Blocklisting.Blocklist> GetBlocklist(PagingSpec<NzbDrone.Core.Blocklisting.Blocklist> pagingSpec, HashSet<int> seriesIds, DownloadProtocol? protocol)
        {
            var ascending = pagingSpec.SortDirection == SortDirection.Ascending;
            var orderByFunc = GetOrderByFunc(pagingSpec);

            var blocklist = _blocklistService.GetBlocklist();

            var hasSeriesIdFilter = seriesIds.Any();
            var fullBlocklist = blocklist.Where(b =>
            {
                var include = true;

                if (hasSeriesIdFilter)
                {
                    include &= seriesIds.Contains(b.SeriesId);
                }

                if (include && protocol.HasValue)
                {
                    include &= b.Protocol == protocol.Value;
                }

                return include;
            }).ToList();

            IOrderedEnumerable<NzbDrone.Core.Blocklisting.Blocklist> ordered;

            if (pagingSpec.SortKey == "date")
            {
                ordered = ascending
                    ? fullBlocklist.OrderBy(b => b.Date)
                    : fullBlocklist.OrderByDescending(b => b.Date);
            }
            else if (pagingSpec.SortKey == "indexer")
            {
                ordered = ascending
                    ? fullBlocklist.OrderBy(b => b.Indexer, StringComparer.InvariantCultureIgnoreCase)
                    : fullBlocklist.OrderByDescending(b => b.Indexer, StringComparer.InvariantCultureIgnoreCase);
            }
            else
            {
                ordered = ascending ? fullBlocklist.OrderBy(orderByFunc) : fullBlocklist.OrderByDescending(orderByFunc);
            }

            pagingSpec.Records = ordered.Skip((pagingSpec.Page - 1) * pagingSpec.PageSize).Take(pagingSpec.PageSize).ToList();
            pagingSpec.TotalRecords = fullBlocklist.Count;

            if (pagingSpec.Records.Empty() && pagingSpec.Page > 1)
            {
                pagingSpec.Page = (int)Math.Max(Math.Ceiling((decimal)(pagingSpec.TotalRecords / pagingSpec.PageSize)), 1);
                pagingSpec.Records = ordered.Skip((pagingSpec.Page - 1) * pagingSpec.PageSize).Take(pagingSpec.PageSize).ToList();
            }

            return pagingSpec;
        }

        private Func<NzbDrone.Core.Blocklisting.Blocklist, object> GetOrderByFunc(PagingSpec<NzbDrone.Core.Blocklisting.Blocklist> pagingSpec)
        {
            switch (pagingSpec.SortKey)
            {
                case "series.sortTitle":
                    return q => q.Series?.SortTitle ?? q.Series?.Title;
                case "sourceTitle":
                    return q => q.SourceTitle;
                default:
                    return q => q.Date;
            }
        }

        [RestDeleteById]
        public void DeleteBlocklist(int id)
        {
            _blocklistService.Delete(id);
        }

        [HttpDelete("bulk")]
        [Produces("application/json")]
        public object Remove([FromBody] BlocklistBulkResource resource)
        {
            _blocklistService.Delete(resource.Ids);

            return new { };
        }
    }
}
