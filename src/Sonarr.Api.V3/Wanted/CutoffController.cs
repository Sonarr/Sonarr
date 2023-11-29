using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Tv;
using NzbDrone.SignalR;
using Sonarr.Api.V3.Episodes;
using Sonarr.Http;
using Sonarr.Http.Extensions;

namespace Sonarr.Api.V3.Wanted
{
    [V3ApiController("wanted/cutoff")]
    public class CutoffController : EpisodeControllerWithSignalR
    {
        private readonly IEpisodeCutoffService _episodeCutoffService;

        public CutoffController(IEpisodeCutoffService episodeCutoffService,
                            IEpisodeService episodeService,
                            ISeriesService seriesService,
                            IUpgradableSpecification upgradableSpecification,
                            ICustomFormatCalculationService formatCalculator,
                            IBroadcastSignalRMessage signalRBroadcaster)
            : base(episodeService, seriesService, upgradableSpecification, formatCalculator, signalRBroadcaster)
        {
            _episodeCutoffService = episodeCutoffService;
        }

        [HttpGet]
        [Produces("application/json")]
        public PagingResource<EpisodeResource> GetCutoffUnmetEpisodes([FromQuery] PagingRequestResource paging, bool includeSeries = false, bool includeEpisodeFile = false, bool includeImages = false, bool monitored = true)
        {
            var pagingResource = new PagingResource<EpisodeResource>(paging);
            var pagingSpec = new PagingSpec<Episode>
            {
                Page = pagingResource.Page,
                PageSize = pagingResource.PageSize,
                SortKey = pagingResource.SortKey,
                SortDirection = pagingResource.SortDirection
            };

            if (monitored)
            {
                pagingSpec.FilterExpressions.Add(v => v.Monitored == true && v.Series.Monitored == true);
            }
            else
            {
                pagingSpec.FilterExpressions.Add(v => v.Monitored == false || v.Series.Monitored == false);
            }

            var resource = pagingSpec.ApplyToPage(_episodeCutoffService.EpisodesWhereCutoffUnmet, v => MapToResource(v, includeSeries, includeEpisodeFile, includeImages));

            return resource;
        }
    }
}
