using System;
using Nancy;
using NzbDrone.Api.Episodes;
using Sonarr.Http.Extensions;
using NzbDrone.Api.Series;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
using Sonarr.Http;

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

            Post["/failed"] = x => MarkAsFailed();
        }

        protected HistoryResource MapToResource(Core.History.History model)
        {
            var resource = model.ToResource();

            resource.Series = model.Series.ToResource();
            resource.Episode = model.Episode.ToResource();

            if (model.Series != null)
            {
                resource.QualityCutoffNotMet = _upgradableSpecification.CutoffNotMet(model.Series.Profile.Value, 
                                                                                     model.Series.LanguageProfile, 
                                                                                     model.Quality, 
                                                                                     model.Language);
            }

            return resource;
        }

        private PagingResource<HistoryResource> GetHistory(PagingResource<HistoryResource> pagingResource)
        {
            var episodeId = Request.Query.EpisodeId;

            var pagingSpec = pagingResource.MapToPagingSpec<HistoryResource, Core.History.History>("date", SortDirection.Descending);

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

            return ApplyToPage(_historyService.Paged, pagingSpec, MapToResource);
        }

        private Response MarkAsFailed()
        {
            var id = (int)Request.Form.Id;
            _failedDownloadService.MarkAsFailed(id);
            return new object().AsResponse();
        }
    }
}
