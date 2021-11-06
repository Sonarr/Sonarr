using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Tv;
using NzbDrone.SignalR;
using Sonarr.Api.V3.Episodes;
using Sonarr.Http;
using Sonarr.Http.Extensions;

namespace Sonarr.Api.V3.Wanted
{
    [V3ApiController("wanted/missing")]
    public class MissingController : EpisodeControllerWithSignalR
    {
        public MissingController(IEpisodeService episodeService,
                             ISeriesService seriesService,
                             IUpgradableSpecification upgradableSpecification,
                             IBroadcastSignalRMessage signalRBroadcaster)
            : base(episodeService, seriesService, upgradableSpecification, signalRBroadcaster)
        {
        }

        [HttpGet]
        public PagingResource<EpisodeResource> GetMissingEpisodes(bool includeSeries = false, bool includeImages = false)
        {
            var pagingResource = Request.ReadPagingResourceFromRequest<EpisodeResource>();
            var pagingSpec = new PagingSpec<Episode>
            {
                Page = pagingResource.Page,
                PageSize = pagingResource.PageSize,
                SortKey = pagingResource.SortKey,
                SortDirection = pagingResource.SortDirection
            };

            var monitoredFilter = pagingResource.Filters.FirstOrDefault(f => f.Key == "monitored");

            if (monitoredFilter != null && monitoredFilter.Value == "false")
            {
                pagingSpec.FilterExpressions.Add(v => v.Monitored == false || v.Series.Monitored == false);
            }
            else
            {
                pagingSpec.FilterExpressions.Add(v => v.Monitored == true && v.Series.Monitored == true);
            }

            var resource = pagingSpec.ApplyToPage(_episodeService.EpisodesWithoutFiles, v => MapToResource(v, includeSeries, false, includeImages));

            return resource;
        }
    }
}
