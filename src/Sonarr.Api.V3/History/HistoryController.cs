using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
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
        private readonly IUpgradableSpecification _upgradableSpecification;
        private readonly IFailedDownloadService _failedDownloadService;

        public HistoryController(IHistoryService historyService,
                             IUpgradableSpecification upgradableSpecification,
                             IFailedDownloadService failedDownloadService)
        {
            _historyService = historyService;
            _upgradableSpecification = upgradableSpecification;
            _failedDownloadService = failedDownloadService;
        }

        protected HistoryResource MapToResource(EpisodeHistory model, bool includeSeries, bool includeEpisode)
        {
            var resource = model.ToResource();

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
                resource.LanguageCutoffNotMet = _upgradableSpecification.LanguageCutoffNotMet(model.Series.LanguageProfile.Value, model.Language);
            }

            return resource;
        }

        [HttpGet]
        public PagingResource<HistoryResource> GetHistory(bool includeSeries, bool includeEpisode)
        {
            var pagingResource = Request.ReadPagingResourceFromRequest<HistoryResource>();
            var pagingSpec = pagingResource.MapToPagingSpec<HistoryResource, EpisodeHistory>("date", SortDirection.Descending);

            var eventTypeFilter = pagingResource.Filters.FirstOrDefault(f => f.Key == "eventType");
            var episodeIdFilter = pagingResource.Filters.FirstOrDefault(f => f.Key == "episodeId");
            var downloadIdFilter = pagingResource.Filters.FirstOrDefault(f => f.Key == "downloadId");

            if (eventTypeFilter != null)
            {
                var filterValue = (EpisodeHistoryEventType)Convert.ToInt32(eventTypeFilter.Value);
                pagingSpec.FilterExpressions.Add(v => v.EventType == filterValue);
            }

            if (episodeIdFilter != null)
            {
                var episodeId = Convert.ToInt32(episodeIdFilter.Value);
                pagingSpec.FilterExpressions.Add(h => h.EpisodeId == episodeId);
            }

            if (downloadIdFilter != null)
            {
                var downloadId = downloadIdFilter.Value;
                pagingSpec.FilterExpressions.Add(h => h.DownloadId == downloadId);
            }

            return pagingSpec.ApplyToPage(_historyService.Paged, h => MapToResource(h, includeSeries, includeEpisode));
        }

        [HttpGet("since")]
        public List<HistoryResource> GetHistorySince(DateTime date, EpisodeHistoryEventType? eventType = null, bool includeSeries = false, bool includeEpisode = false)
        {
            return _historyService.Since(date, eventType).Select(h => MapToResource(h, includeSeries, includeEpisode)).ToList();
        }

        [HttpGet("series")]
        public List<HistoryResource> GetSeriesHistory(int seriesId, int? seasonNumber, EpisodeHistoryEventType? eventType = null, bool includeSeries = false, bool includeEpisode = false)
        {
            if (seasonNumber.HasValue)
            {
                return _historyService.GetBySeason(seriesId, seasonNumber.Value, eventType).Select(h => MapToResource(h, includeSeries, includeEpisode)).ToList();
            }

            return _historyService.GetBySeries(seriesId, eventType).Select(h => MapToResource(h, includeSeries, includeEpisode)).ToList();
        }

        [HttpPost("failed/{id}")]
        public object MarkAsFailed([FromRoute] int id)
        {
            _failedDownloadService.MarkAsFailed(id);
            return new { };
        }
    }
}
