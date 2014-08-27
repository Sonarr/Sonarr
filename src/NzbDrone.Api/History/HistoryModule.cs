using System;
using Nancy;
using NzbDrone.Api.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;

namespace NzbDrone.Api.History
{
    public class HistoryModule : NzbDroneRestModule<HistoryResource>
    {
        private readonly IHistoryService _historyService;
        private readonly IQualityUpgradableSpecification _qualityUpgradableSpecification;
        private readonly IDownloadTrackingService _downloadTrackingService;

        public HistoryModule(IHistoryService historyService,
                             IQualityUpgradableSpecification qualityUpgradableSpecification,
                             IDownloadTrackingService downloadTrackingService)
        {
            _historyService = historyService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
            _downloadTrackingService = downloadTrackingService;
            GetResourcePaged = GetHistory;

            Post["/failed"] = x => MarkAsFailed();
        }

        protected override HistoryResource ToResource<TModel>(TModel model)
        {
            var resource = base.ToResource<TModel>(model);

            var history = model as NzbDrone.Core.History.History;

            if (history != null && history.Series != null)
            {
                resource.QualityCutoffNotMet = _qualityUpgradableSpecification.CutoffNotMet(history.Series.Profile.Value, history.Quality);
            }

            return resource;
        }

        private PagingResource<HistoryResource> GetHistory(PagingResource<HistoryResource> pagingResource)
        {
            var episodeId = Request.Query.EpisodeId;

            var pagingSpec = new PagingSpec<Core.History.History>
                                 {
                                     Page = pagingResource.Page,
                                     PageSize = pagingResource.PageSize,
                                     SortKey = pagingResource.SortKey,
                                     SortDirection = pagingResource.SortDirection
                                 };

            if (pagingResource.FilterKey == "eventType")
            {
                var filterValue = (HistoryEventType)Convert.ToInt32(pagingResource.FilterValue);
                pagingSpec.FilterExpression = v => v.EventType == filterValue;
            }

            if (episodeId.HasValue)
            {
                int i = (int)episodeId;
                pagingSpec.FilterExpression = h => h.EpisodeId == i;
            }

            return ApplyToPage(_historyService.Paged, pagingSpec);
        }

        private Response MarkAsFailed()
        {
            var id = (int)Request.Form.Id;
            _downloadTrackingService.MarkAsFailed(id);
            return new Object().AsResponse();
        }
    }
}