using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.DecisionEngine.Specifications;
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
            Put(@"/(?<id>[\d]{1,10})",  x => SetEpisodeMonitored(x.Id));
            Put("/monitor",  x => SetEpisodesMonitored());
        }

        private List<EpisodeResource> GetEpisodes()
        {
            var seriesIdQuery = Request.Query.SeriesId;
            var episodeIdsQuery = Request.Query.EpisodeIds;
            var episodeFileIdQuery = Request.Query.EpisodeFileId;
            var includeImages = Request.GetBooleanQueryParameter("includeImages", false);

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
            else if (episodeIdsQuery.HasValue)
            {
                string episodeIdsValue = episodeIdsQuery.Value.ToString();

                var episodeIds = episodeIdsValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                .Select(e => Convert.ToInt32(e))
                                                .ToList();

                return MapToResource(_episodeService.GetEpisodes(episodeIds), false, false, includeImages);
            }
            else if (episodeFileIdQuery.HasValue)
            {
                int episodeFileId = Convert.ToInt32(episodeFileIdQuery.Value);

                return MapToResource(_episodeService.GetEpisodesByFileId(episodeFileId), false, false, includeImages);
            }

            throw new BadRequestException("seriesId or episodeIds must be provided");
        }

        private object SetEpisodeMonitored(int id)
        {
            var resource = Request.Body.FromJson<EpisodeResource>();
            _episodeService.SetEpisodeMonitored(id, resource.Monitored);

            resource = MapToResource(_episodeService.GetEpisode(id), false, false, false);

            return ResponseWithCode(resource, HttpStatusCode.Accepted);
        }

        private object SetEpisodesMonitored()
        {
            var includeImages = Request.GetBooleanQueryParameter("includeImages", false);
            var resource = Request.Body.FromJson<EpisodesMonitoredResource>();

            if (resource.EpisodeIds.Count == 1)
            {
                _episodeService.SetEpisodeMonitored(resource.EpisodeIds.First(), resource.Monitored);
            }
            else
            {
                _episodeService.SetMonitored(resource.EpisodeIds, resource.Monitored);
            }

            var resources = MapToResource(_episodeService.GetEpisodes(resource.EpisodeIds), false, false, includeImages);

            return ResponseWithCode(resources, HttpStatusCode.Accepted);
        }
    }
}
