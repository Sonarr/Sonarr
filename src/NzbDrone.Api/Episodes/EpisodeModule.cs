using System.Collections.Generic;
using NzbDrone.Api.REST;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Tv;
using NzbDrone.Api.Mapping;
using NzbDrone.Core.DecisionEngine;

namespace NzbDrone.Api.Episodes
{
    public class EpisodeModule : EpisodeModuleWithSignalR
    {
        private readonly ISeriesService _seriesService;
        private readonly IEpisodeService _episodeService;
        private readonly IQualityUpgradableSpecification _qualityUpgradableSpecification;

        public EpisodeModule(ICommandExecutor commandExecutor, ISeriesService seriesService, IEpisodeService episodeService, IQualityUpgradableSpecification qualityUpgradableSpecification)
            : base(episodeService, commandExecutor)
        {
            _seriesService = seriesService;
            _episodeService = episodeService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;

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

            var resources = new List<EpisodeResource>();
            foreach (var episode in _episodeService.GetEpisodeBySeries(seriesId.Value))
            {
                var resource = episode.InjectTo<EpisodeResource>();
                if (episode.EpisodeFile.IsLoaded)
                {
                    resource.EpisodeFile.QualityCutoffNotMet = _qualityUpgradableSpecification.CutoffNotMet(series.Profile.Value, episode.EpisodeFile.Value.Quality);
                }

                resources.Add(resource);
            }

            return resources;
        }

        private void SetMonitored(EpisodeResource episodeResource)
        {
            _episodeService.SetEpisodeMonitored(episodeResource.Id, episodeResource.Monitored);
        }
    }
}