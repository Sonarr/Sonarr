using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Tv;
using NzbDrone.SignalR;
using Sonarr.Api.V5.Episodes;
using Sonarr.Http;
using Sonarr.Http.Extensions;

namespace Sonarr.Api.V5.Wanted;

[V5ApiController("wanted/cutoff")]
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
    public Ok<PagingResource<EpisodeResource>> GetCutoffUnmetEpisodes(
        [FromQuery] PagingRequestResource paging,
        [FromQuery] bool monitored = true,
        [FromQuery] List<int>? seriesIds = null,
        [FromQuery] List<int>? qualityProfileIds = null,
        [FromQuery] List<SeriesTypes>? seriesType = null,
        [FromQuery] HashSet<int>? seriesTags = null,
        [FromQuery] List<int>? quality = null,
        [FromQuery] CutoffSubresource[]? includeSubresources = null)
    {
        var pagingResource = new PagingResource<EpisodeResource>(paging);
        var pagingSpec = pagingResource.MapToPagingSpec<EpisodeResource, Episode>(
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "episodes.airDateUtc",
                "episodes.lastSearchTime",
                "series.sortTitle"
            },
            "episodes.airDateUtc",
            SortDirection.Ascending);

        if (monitored)
        {
            pagingSpec.FilterExpressions.Add(v => v.Monitored == true && v.Series.Monitored == true);
        }
        else
        {
            pagingSpec.FilterExpressions.Add(v => v.Monitored == false || v.Series.Monitored == false);
        }

        if (seriesIds?.Any() == true)
        {
            pagingSpec.FilterExpressions.Add(e => seriesIds.Contains(e.SeriesId));
        }

        if (qualityProfileIds?.Any() == true)
        {
            pagingSpec.FilterExpressions.Add(e => qualityProfileIds.Contains(e.Series.QualityProfileId));
        }

        if (seriesType?.Any() == true)
        {
            pagingSpec.FilterExpressions.Add(e => seriesType.Contains(e.Series.SeriesType));
        }

        var includeSeries = includeSubresources.Contains(CutoffSubresource.Series);
        var includeEpisodeFile = includeSubresources.Contains(CutoffSubresource.EpisodeFile);
        var includeImages = includeSubresources.Contains(CutoffSubresource.Images);

        var resource = pagingSpec.ApplyToPage(spec => _episodeCutoffService.EpisodesWhereCutoffUnmet(spec, seriesTags, quality), v => MapToResource(v, includeSeries, includeEpisodeFile, includeImages));

        return TypedResults.Ok(resource);
    }
}
