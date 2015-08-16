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
        private readonly IFailedDownloadService _failedDownloadService;

        public HistoryModule(IHistoryService historyService,
                             IQualityUpgradableSpecification qualityUpgradableSpecification,
                             IFailedDownloadService failedDownloadService)
        {
            _historyService = historyService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
            _failedDownloadService = failedDownloadService;
            GetResourcePaged = GetHistory;

            Post["/failed"] = x => MarkAsFailed();
        }

        protected override HistoryResource ToResource<TModel>(TModel model)
        {
            var resource = base.ToResource(model);

            var history = model as Core.History.History;

            if (history != null && history.Series != null)
            {
                resource.QualityCutoffNotMet = _qualityUpgradableSpecification.CutoffNotMet(history.Series.Profile.Value, history.Quality);
            }

            return resource;
        }

        private PagingResource<HistoryResource> GetHistory(PagingResource<HistoryResource> pagingResource)
        {
            var episodeId = Request.Query.EpisodeId;
            var movieId = Request.Query.movieId;
            var mediaType = Request.Query.mediaType;

            var pagingSpec = new PagingSpec<Core.History.History>
                                 {
                                     Page = pagingResource.Page,
                                     PageSize = pagingResource.PageSize,
                                     SortKey = pagingResource.SortKey,
                                     SortDirection = pagingResource.SortDirection
                                 };

            if (mediaType.HasValue && pagingResource.FilterKey == "eventType")
            {
                var type = (MediaType)Convert.ToInt32(mediaType.Value);
                var filterValue = (HistoryEventType)Convert.ToInt32(pagingResource.FilterValue);
                switch (type)
                {
                    case MediaType.Series:
                        pagingSpec.FilterExpression = h => h.SeriesId > 0 && h.EventType == filterValue;
                        break;
                    case MediaType.Movies:
                        pagingSpec.FilterExpression = h => h.MovieId > 0 && h.EventType == filterValue;
                        break;
                }
            }
            else if (mediaType.HasValue)
            {
                var type = (MediaType)Convert.ToInt32(mediaType.Value);
                switch (type)
                {
                    case MediaType.Series:
                        pagingSpec.FilterExpression = h => h.SeriesId > 0;
                        break;
                    case MediaType.Movies:
                        pagingSpec.FilterExpression = h => h.MovieId > 0;
                        break;
                }
            }
            else if (pagingResource.FilterKey == "eventType")
            {
                var filterValue = (HistoryEventType)Convert.ToInt32(pagingResource.FilterValue);
                pagingSpec.FilterExpression = v => v.EventType == filterValue;
            }

            if (episodeId.HasValue)
            {
                int i = (int)episodeId;
                pagingSpec.FilterExpression = h => h.EpisodeId == i;
            }

            if (movieId.HasValue)
            {
                int i = (int)movieId;
                pagingSpec.FilterExpression = h => h.MovieId == i;
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