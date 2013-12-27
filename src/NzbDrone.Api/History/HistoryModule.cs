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
        private readonly IFailedDownloadService _failedDownloadService;

        public HistoryModule(IHistoryService historyService, IFailedDownloadService failedDownloadService)
        {
            _historyService = historyService;
            _failedDownloadService = failedDownloadService;
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

            //This is a hack to deal with backgrid setting the sortKey to the column name instead of sortValue
            if (pagingSpec.SortKey.Equals("series", StringComparison.InvariantCultureIgnoreCase))
            {
                pagingSpec.SortKey = "series.title";
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
            _failedDownloadService.MarkAsFailed(id);
            return new Object().AsResponse();
        }
    }
}