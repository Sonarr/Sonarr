using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
using Sonarr.Api.V3.Episodes;
using Sonarr.Api.V3.Series;
using Sonarr.Http;
using Sonarr.Http.Extensions;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.History
{
    public class HistoryModule : SonarrRestModule<HistoryResource>
    {
        private readonly IHistoryService _historyService;
        private readonly IUpgradableSpecification _upgradableSpecification;
        private readonly IFailedDownloadService _failedDownloadService;

        public HistoryModule(IHistoryService historyService,
                             IUpgradableSpecification upgradableSpecification,
                             IFailedDownloadService failedDownloadService)
        {
            _historyService = historyService;
            _upgradableSpecification = upgradableSpecification;
            _failedDownloadService = failedDownloadService;
            GetResourcePaged = GetHistory;

            Get("/since",  x => GetHistorySince());
            Get("/series",  x => GetSeriesHistory());
            Post("/failed",  x => MarkAsFailed());
            Post(@"/failed/(?<id>[\d]{1,10})", x => MarkAsFailed((int)x.Id));
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
                resource.LanguageCutoffNotMet = _upgradableSpecification.LanguageCutoffNotMet(model.Series.LanguageProfile, model.Language);
            }

            return resource;
        }

        private PagingResource<HistoryResource> GetHistory(PagingResource<HistoryResource> pagingResource)
        {
            var pagingSpec = pagingResource.MapToPagingSpec<HistoryResource, EpisodeHistory>("date", SortDirection.Descending);
            var includeSeries = Request.GetBooleanQueryParameter("includeSeries");
            var includeEpisode = Request.GetBooleanQueryParameter("includeEpisode");

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

            return ApplyToPage(_historyService.Paged, pagingSpec, h => MapToResource(h, includeSeries, includeEpisode));
        }

        private List<HistoryResource> GetHistorySince()
        {
            var queryDate = Request.Query.Date;
            var queryEventType = Request.Query.EventType;

            if (!queryDate.HasValue)
            {
                throw new BadRequestException("date is missing");
            }

            DateTime date = DateTime.Parse(queryDate.Value);
            EpisodeHistoryEventType? eventType = null;
            var includeSeries = Request.GetBooleanQueryParameter("includeSeries");
            var includeEpisode = Request.GetBooleanQueryParameter("includeEpisode");

            if (queryEventType.HasValue)
            {
                eventType = (EpisodeHistoryEventType)Convert.ToInt32(queryEventType.Value);
            }

            return _historyService.Since(date, eventType).Select(h => MapToResource(h, includeSeries, includeEpisode)).ToList();
        }

        private List<HistoryResource> GetSeriesHistory()
        {
            var querySeriesId = Request.Query.SeriesId;
            var querySeasonNumber = Request.Query.SeasonNumber;
            var queryEventType = Request.Query.EventType;

            if (!querySeriesId.HasValue)
            {
                throw new BadRequestException("seriesId is missing");
            }

            int seriesId = Convert.ToInt32(querySeriesId.Value);
            EpisodeHistoryEventType? eventType = null;
            var includeSeries = Request.GetBooleanQueryParameter("includeSeries");
            var includeEpisode = Request.GetBooleanQueryParameter("includeEpisode");

            if (queryEventType.HasValue)
            {
                eventType = (EpisodeHistoryEventType)Convert.ToInt32(queryEventType.Value);
            }

            if (querySeasonNumber.HasValue)
            {
                int seasonNumber = Convert.ToInt32(querySeasonNumber.Value);

                return _historyService.GetBySeason(seriesId, seasonNumber, eventType).Select(h => MapToResource(h, includeSeries, includeEpisode)).ToList();
            }

            return _historyService.GetBySeries(seriesId, eventType).Select(h => MapToResource(h, includeSeries, includeEpisode)).ToList();
        }

        // v4 TODO: Getting the ID from the form is atypical, consider removing.
        private object MarkAsFailed()
        {
            var id = (int)Request.Form.Id;

            return MarkAsFailed(id);
        }

        private object MarkAsFailed(int id)
        {
            _failedDownloadService.MarkAsFailed(id);

            return new object();
        }
    }
}
