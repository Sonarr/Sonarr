using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Tv;
using NzbDrone.SignalR;
using Sonarr.Http.Extensions;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.Episodes
{
    public class EpisodeModule : EpisodeModuleWithSignalR
    {
        public EpisodeModule(ISeriesService seriesService,
                             IEpisodeService episodeService,
                             IUpgradableSpecification upgradableSpecification,
                             IBroadcastSignalRMessage signalRBroadcaster)
            : base(episodeService, seriesService, upgradableSpecification, signalRBroadcaster)
        {
            GetResourceAll = GetEpisodes;
            Put[@"/(?<id>[\d]{1,10})"] = x => SetEpisodeMonitored(x.Id);
            Put["/monitor"] = x => SetEpisodesMonitored();
        }

        private List<EpisodeResource> GetEpisodes()
        {
            var seriesIdQuery = Request.Query.SeriesId;
            var episodeIdsQuery = Request.Query.EpisodeIds;
            var includeImages = Request.GetBooleanQueryParameter("includeImages", false);

            if (!seriesIdQuery.HasValue && !episodeIdsQuery.HasValue)
            {
                throw new BadRequestException("seriesId or episodeIds must be provided");
            }

            if (seriesIdQuery.HasValue)
            {
                int seriesId = Convert.ToInt32(seriesIdQuery.Value);
                var seasonNumber = Request.Query.SeasonNumber.HasValue ? (int)Request.Query.SeasonNumber : (int?)null;

                if (seasonNumber.HasValue)
                {
                    return MapToResource(_episodeService.GetEpisodesBySeason(seriesId, seasonNumber.Value), false, false, includeImages);
                }

                return MapToResource(_episodeService.GetEpisodeBySeries(seriesId), false, false, includeImages);
            }

            string episodeIdsValue = episodeIdsQuery.Value.ToString();

            var episodeIds = episodeIdsValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(e => Convert.ToInt32(e))
                                            .ToList();

            return MapToResource(_episodeService.GetEpisodes(episodeIds), false, false, includeImages);
        }

        private Response SetEpisodeMonitored(int id)
        {
            var resource = Request.Body.FromJson<EpisodeResource>();
            _episodeService.SetEpisodeMonitored(id, resource.Monitored);

            return MapToResource(_episodeService.GetEpisode(id), false, false, false).AsResponse(HttpStatusCode.Accepted);
        }

        private Response SetEpisodesMonitored()
        {
            var includeImages = Request.GetBooleanQueryParameter("includeImages", false);
            var resource = Request.Body.FromJson<EpisodesMonitoredResource>();

            _episodeService.SetMonitored(resource.EpisodeIds, resource.Monitored);

            return MapToResource(_episodeService.GetEpisodes(resource.EpisodeIds), false, false, includeImages)
                .AsResponse(HttpStatusCode.Accepted);
        }
    }
}
