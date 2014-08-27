using System.Collections.Generic;
using NzbDrone.Api.REST;
using NzbDrone.Core.Tv;
using NzbDrone.Api.Mapping;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.SignalR;

namespace NzbDrone.Api.Episodes
{
    public class EpisodeModule : EpisodeModuleWithSignalR
    {
        protected readonly ISeriesService _seriesService;

        public EpisodeModule(ISeriesService seriesService,
                             IEpisodeService episodeService,
                             IQualityUpgradableSpecification qualityUpgradableSpecification,
                             IBroadcastSignalRMessage signalRBroadcaster)
            : base(episodeService, qualityUpgradableSpecification, signalRBroadcaster)
        {
            _seriesService = seriesService;

            GetResourceAll = GetEpisodes;
            UpdateResource = SetMonitored;
        }

        private List<EpisodeResource> GetEpisodes()
        {
            var seriesId = (int?)Request.Query.SeriesId;

            if (seriesId == null)
            {
                throw new BadRequestException("seriesId is missing");
            }

            var series = _seriesService.GetSeries(seriesId.Value);

            var resources = ToListResource(_episodeService.GetEpisodeBySeries(seriesId.Value));

            return resources;
        }

        private void SetMonitored(EpisodeResource episodeResource)
        {
            _episodeService.SetEpisodeMonitored(episodeResource.Id, episodeResource.Monitored);
        }
    }
}