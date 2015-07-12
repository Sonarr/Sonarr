using System;
using Nancy;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
using Sonarr.Api.V3.Episodes;
using Sonarr.Api.V3.Series;
using Sonarr.Http;
using Sonarr.Http.Extensions;

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

            Post["/failed"] = x => MarkAsFailed();
        }

        protected HistoryResource MapToResource(NzbDrone.Core.History.History model, bool includeSeries, bool includeEpisode)
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
                resource.QualityCutoffNotMet = _upgradableSpecification.CutoffNotMet(model.Series.Profile.Value,
                                                                                     model.Series.LanguageProfile,
                                                                                     model.Quality,
                                                                                     model.Language);
            }

            return resource;
        }

        private PagingResource<HistoryResource> GetHistory(PagingResource<HistoryResource> pagingResource)
        {
            var pagingSpec = pagingResource.MapToPagingSpec<HistoryResource, NzbDrone.Core.History.History>("date", SortDirection.Descending);
            var includeSeries = Request.GetBooleanQueryParameter("includeSeries");
            var includeEpisode = Request.GetBooleanQueryParameter("includeEpisode");

            if (pagingResource.FilterKey == "eventType")
            {
                var filterValue = (HistoryEventType)Convert.ToInt32(pagingResource.FilterValue);
                pagingSpec.FilterExpression = v => v.EventType == filterValue;
            }

            if (pagingResource.FilterKey == "episodeId")
            {
                int episodeId = Convert.ToInt32(pagingResource.FilterValue);
                pagingSpec.FilterExpression = h => h.EpisodeId == episodeId;
            }

            return ApplyToPage(_historyService.Paged, pagingSpec, h => MapToResource(h, includeSeries, includeEpisode));
        }

        private Response MarkAsFailed()
        {
            var id = (int)Request.Form.Id;
            _failedDownloadService.MarkAsFailed(id);
            return new object().AsResponse();
        }
    }
}
