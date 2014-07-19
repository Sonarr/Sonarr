using System;
using Nancy;
using NzbDrone.Api.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;

namespace NzbDrone.Api.History
{
    public class HistoryModule : NzbDroneRestModule<HistoryResource>
    {
        private readonly IHistoryService _historyService;
        private readonly IDownloadTrackingService _downloadTrackingService;

        public HistoryModule(IHistoryService historyService, IDownloadTrackingService downloadTrackingService)
        {
            _historyService = historyService;
            _downloadTrackingService = downloadTrackingService;
            GetResourcePaged = GetHistory;

            Post["/failed"] = x => MarkAsFailed();
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