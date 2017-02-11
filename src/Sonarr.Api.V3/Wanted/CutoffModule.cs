using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Tv;
using NzbDrone.SignalR;
using Sonarr.Api.V3.Episodes;
using Sonarr.Http;
using Sonarr.Http.Extensions;

namespace Sonarr.Api.V3.Wanted
{
    public class CutoffModule : EpisodeModuleWithSignalR
    {
        private readonly IEpisodeCutoffService _episodeCutoffService;

        public CutoffModule(IEpisodeCutoffService episodeCutoffService,
                            IEpisodeService episodeService,
                            ISeriesService seriesService,
                            IQualityUpgradableSpecification qualityUpgradableSpecification,
                            IBroadcastSignalRMessage signalRBroadcaster)
            : base(episodeService, seriesService, qualityUpgradableSpecification, signalRBroadcaster, "wanted/cutoff")
        {
            _episodeCutoffService = episodeCutoffService;
            GetResourcePaged = GetCutoffUnmetEpisodes;
        }

        private PagingResource<EpisodeResource> GetCutoffUnmetEpisodes(PagingResource<EpisodeResource> pagingResource)
        {
            var pagingSpec = new PagingSpec<Episode>
            {
                Page = pagingResource.Page,
                PageSize = pagingResource.PageSize,
                SortKey = pagingResource.SortKey,
                SortDirection = pagingResource.SortDirection
            };

            var includeSeries = Request.GetBooleanQueryParameter("includeSeries");
            var includeEpisodeFile = Request.GetBooleanQueryParameter("includeEpisodeFile");
            var includeImages = Request.GetBooleanQueryParameter("includeImages");
            var filter = pagingResource.Filters.FirstOrDefault(f => f.Key == "monitored");

            if (filter != null && filter.Value == "false")
            {
                pagingSpec.FilterExpressions.Add(v => v.Monitored == false || v.Series.Monitored == false);
            }
            else
            {
                pagingSpec.FilterExpressions.Add(v => v.Monitored == true && v.Series.Monitored == true);
            }

            var resource = ApplyToPage(_episodeCutoffService.EpisodesWhereCutoffUnmet, pagingSpec, v => MapToResource(v, includeSeries, includeEpisodeFile, includeImages));

            return resource;
        }
    }
}
