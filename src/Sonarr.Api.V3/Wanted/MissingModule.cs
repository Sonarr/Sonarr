using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Tv;
using NzbDrone.SignalR;
using Sonarr.Api.V3.Episodes;
using Sonarr.Http;
using Sonarr.Http.Extensions;

namespace Sonarr.Api.V3.Wanted
{
    public class MissingModule : EpisodeModuleWithSignalR
    {
        public MissingModule(IEpisodeService episodeService,
                             ISeriesService seriesService,
                             IQualityUpgradableSpecification qualityUpgradableSpecification,
                             IBroadcastSignalRMessage signalRBroadcaster)
            : base(episodeService, seriesService, qualityUpgradableSpecification, signalRBroadcaster, "wanted/missing")
        {
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

            var includeSeries = Request.GetBooleanQueryParameter("includeSeries");

            if (pagingResource.FilterKey == "monitored" && pagingResource.FilterValue == "false")
            {
                pagingSpec.FilterExpression = v => v.Monitored == false || v.Series.Monitored == false;
            }
            else
            {
                pagingSpec.FilterExpression = v => v.Monitored == true && v.Series.Monitored == true;
            }

            var resource = ApplyToPage(_episodeService.EpisodesWithoutFiles, pagingSpec, v => MapToResource(v, includeSeries, false));

            return resource;
        }
    }
}
