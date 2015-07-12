using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Api.Episodes;
using Sonarr.Http.Extensions;
using NzbDrone.Api.Series;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
using Sonarr.Http;
using Sonarr.Http.REST;

namespace NzbDrone.Api.History
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

            Get["/since"] = x => GetHistorySince();
            Post["/failed"] = x => MarkAsFailed();
        }

        protected HistoryResource MapToResource(Core.History.History model)
        {
            var resource = model.ToResource();

            resource.Series = model.Series.ToResource();
            resource.Episode = model.Episode.ToResource();

            if (model.Series != null)
            {
                resource.QualityCutoffNotMet = _upgradableSpecification.QualityCutoffNotMet(model.Series.Profile.Value, model.Quality);
            }

            return resource;
        }

        private PagingResource<HistoryResource> GetHistory(PagingResource<HistoryResource> pagingResource)
        {
            var episodeId = Request.Query.EpisodeId;
            var pagingSpec = pagingResource.MapToPagingSpec<HistoryResource, Core.History.History>("date", SortDirection.Descending);
            var filter = pagingResource.Filters.FirstOrDefault();

            if (filter != null && filter.Key == "eventType")
            {
                var filterValue = (HistoryEventType)Convert.ToInt32(filter.Value);
                pagingSpec.FilterExpressions.Add(v => v.EventType == filterValue);
            }

            if (episodeId.HasValue)
            {
                int i = (int)episodeId;
                pagingSpec.FilterExpressions.Add(h => h.EpisodeId == i);
            }

            return ApplyToPage(_historyService.Paged, pagingSpec, MapToResource);
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
            HistoryEventType? eventType = null;

            if (queryEventType.HasValue)
            {
                eventType = (HistoryEventType)Convert.ToInt32(queryEventType.Value);
            }

            return _historyService.Since(date, eventType).Select(MapToResource).ToList();
        }

        private Response MarkAsFailed()
        {
            var id = (int)Request.Form.Id;
            _failedDownloadService.MarkAsFailed(id);
            return new object().AsResponse();
        }
    }
}
