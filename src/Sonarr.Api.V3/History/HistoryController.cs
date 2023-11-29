using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
using NzbDrone.Core.Tv;
using Sonarr.Api.V3.Episodes;
using Sonarr.Api.V3.Series;
using Sonarr.Http;
using Sonarr.Http.Extensions;

namespace Sonarr.Api.V3.History
{
    [V3ApiController]
    public class HistoryController : Controller
    {
        private readonly IHistoryService _historyService;
        private readonly ICustomFormatCalculationService _formatCalculator;
        private readonly IUpgradableSpecification _upgradableSpecification;
        private readonly IFailedDownloadService _failedDownloadService;
        private readonly ISeriesService _seriesService;

        public HistoryController(IHistoryService historyService,
                             ICustomFormatCalculationService formatCalculator,
                             IUpgradableSpecification upgradableSpecification,
                             IFailedDownloadService failedDownloadService,
                             ISeriesService seriesService)
        {
            _historyService = historyService;
            _formatCalculator = formatCalculator;
            _upgradableSpecification = upgradableSpecification;
            _failedDownloadService = failedDownloadService;
            _seriesService = seriesService;
        }

        protected HistoryResource MapToResource(EpisodeHistory model, bool includeSeries, bool includeEpisode)
        {
            var resource = model.ToResource(_formatCalculator);

            if (includeSeries)
            {
                resource.Series = model.Series.ToResource();
            }

            if (includeEpisode)
            {
                resource.Episode = model.Episode.ToResource();
            }

            if (model.Series != null)
            {
                resource.QualityCutoffNotMet = _upgradableSpecification.QualityCutoffNotMet(model.Series.QualityProfile.Value, model.Quality);
            }

            return resource;
        }

        [HttpGet]
        [Produces("application/json")]
        public PagingResource<HistoryResource> GetHistory([FromQuery] PagingRequestResource paging, bool includeSeries, bool includeEpisode, int? eventType, int? episodeId, string downloadId, [FromQuery] int[] seriesIds = null, [FromQuery] int[] languages = null, [FromQuery] int[] quality = null)
        {
            var pagingResource = new PagingResource<HistoryResource>(paging);
            var pagingSpec = pagingResource.MapToPagingSpec<HistoryResource, EpisodeHistory>("date", SortDirection.Descending);

            if (eventType.HasValue)
            {
                var filterValue = (EpisodeHistoryEventType)eventType.Value;
                pagingSpec.FilterExpressions.Add(v => v.EventType == filterValue);
            }

            if (episodeId.HasValue)
            {
                pagingSpec.FilterExpressions.Add(h => h.EpisodeId == episodeId);
            }

            if (downloadId.IsNotNullOrWhiteSpace())
            {
                pagingSpec.FilterExpressions.Add(h => h.DownloadId == downloadId);
            }

            if (seriesIds != null && seriesIds.Any())
            {
                pagingSpec.FilterExpressions.Add(h => seriesIds.Contains(h.SeriesId));
            }

            return pagingSpec.ApplyToPage(h => _historyService.Paged(pagingSpec, languages, quality), h => MapToResource(h, includeSeries, includeEpisode));
        }

        [HttpGet("since")]
        [Produces("application/json")]
        public List<HistoryResource> GetHistorySince(DateTime date, EpisodeHistoryEventType? eventType = null, bool includeSeries = false, bool includeEpisode = false)
        {
            return _historyService.Since(date, eventType).Select(h => MapToResource(h, includeSeries, includeEpisode)).ToList();
        }

        [HttpGet("series")]
        [Produces("application/json")]
        public List<HistoryResource> GetSeriesHistory(int seriesId, int? seasonNumber, EpisodeHistoryEventType? eventType = null, bool includeSeries = false, bool includeEpisode = false)
        {
            var series = _seriesService.GetSeries(seriesId);

            if (seasonNumber.HasValue)
            {
                return _historyService.GetBySeason(seriesId, seasonNumber.Value, eventType).Select(h =>
                {
                    h.Series = series;

                    return MapToResource(h, includeSeries, includeEpisode);
                }).ToList();
            }

            return _historyService.GetBySeries(seriesId, eventType).Select(h =>
            {
                h.Series = series;

                return MapToResource(h, includeSeries, includeEpisode);
            }).ToList();
        }

        [HttpPost("failed/{id}")]
        public object MarkAsFailed([FromRoute] int id)
        {
            _failedDownloadService.MarkAsFailed(id);
            return new { };
        }
    }
}
