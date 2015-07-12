using System.Linq;
using NzbDrone.Api.Episodes;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Tv;
using NzbDrone.SignalR;
using Sonarr.Http;

namespace NzbDrone.Api.Wanted
{
    public class CutoffModule : EpisodeModuleWithSignalR
    {
        private readonly IEpisodeCutoffService _episodeCutoffService;

        public CutoffModule(IEpisodeCutoffService episodeCutoffService,
                            IEpisodeService episodeService,
                            ISeriesService seriesService,
                            IUpgradableSpecification upgradableSpecification,
                            IBroadcastSignalRMessage signalRBroadcaster)
            : base(episodeService, seriesService, upgradableSpecification, signalRBroadcaster, "wanted/cutoff")
        {
            _episodeCutoffService = episodeCutoffService;
            GetResourcePaged = GetCutoffUnmetEpisodes;
        }

        private PagingResource<EpisodeResource> GetCutoffUnmetEpisodes(PagingResource<EpisodeResource> pagingResource)
        {
            var pagingSpec = pagingResource.MapToPagingSpec<EpisodeResource, Episode>("airDateUtc", SortDirection.Descending);
            var filter = pagingResource.Filters.FirstOrDefault(f => f.Key == "monitored");

            if (filter != null && filter.Value == "false")
            {
                pagingSpec.FilterExpressions.Add(v => v.Monitored == false || v.Series.Monitored == false);
            }
            else
            {
                pagingSpec.FilterExpressions.Add(v => v.Monitored == true && v.Series.Monitored == true);
            }

            var resource = ApplyToPage(_episodeCutoffService.EpisodesWhereCutoffUnmet, pagingSpec, v => MapToResource(v, true, true));

            return resource;
        }
    }
}
