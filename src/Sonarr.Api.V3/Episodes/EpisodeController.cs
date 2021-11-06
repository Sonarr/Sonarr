using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Tv;
using NzbDrone.SignalR;
using Sonarr.Http;
using Sonarr.Http.Extensions;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.Episodes
{
    [V3ApiController]
    public class EpisodeController : EpisodeControllerWithSignalR
    {
        public EpisodeController(ISeriesService seriesService,
                             IEpisodeService episodeService,
                             IUpgradableSpecification upgradableSpecification,
                             IBroadcastSignalRMessage signalRBroadcaster)
            : base(episodeService, seriesService, upgradableSpecification, signalRBroadcaster)
        {
        }

        [HttpGet]
        public List<EpisodeResource> GetEpisodes(int? seriesId, int? seasonNumber, [FromQuery]List<int> episodeIds, int? episodeFileId, bool includeImages = false)
        {
            if (seriesId.HasValue)
            {
                if (seasonNumber.HasValue)
                {
                    return MapToResource(_episodeService.GetEpisodesBySeason(seriesId.Value, seasonNumber.Value), false, false, includeImages);
                }

                return MapToResource(_episodeService.GetEpisodeBySeries(seriesId.Value), false, false, includeImages);
            }
            else if (episodeIds.Any())
            {
                return MapToResource(_episodeService.GetEpisodes(episodeIds), false, false, includeImages);
            }
            else if (episodeFileId.HasValue)
            {
                return MapToResource(_episodeService.GetEpisodesByFileId(episodeFileId.Value), false, false, includeImages);
            }

            throw new BadRequestException("seriesId or episodeIds must be provided");
        }

        [HttpPut("{id}")]
        public IActionResult SetEpisodeMonitored([FromBody] EpisodeResource resource, [FromRoute] int id)
        {
            _episodeService.SetEpisodeMonitored(id, resource.Monitored);

            resource = MapToResource(_episodeService.GetEpisode(id), false, false, false);

            return Accepted(resource);
        }

        [HttpPut("monitor")]
        public IActionResult SetEpisodesMonitored([FromBody] EpisodesMonitoredResource resource)
        {
            var includeImages = Request.GetBooleanQueryParameter("includeImages", false);

            if (resource.EpisodeIds.Count == 1)
            {
                _episodeService.SetEpisodeMonitored(resource.EpisodeIds.First(), resource.Monitored);
            }
            else
            {
                _episodeService.SetMonitored(resource.EpisodeIds, resource.Monitored);
            }

            var resources = MapToResource(_episodeService.GetEpisodes(resource.EpisodeIds), false, false, includeImages);

            return Accepted(resources);
        }
    }
}
