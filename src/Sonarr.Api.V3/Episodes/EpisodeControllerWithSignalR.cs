using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.SignalR;
using Sonarr.Api.V3.EpisodeFiles;
using Sonarr.Api.V3.Series;
using Sonarr.Http.REST;
using Workarr.CustomFormats;
using Workarr.Datastore.Events;
using Workarr.DecisionEngine.Specifications;
using Workarr.Download;
using Workarr.MediaFiles.Events;
using Workarr.Messaging.Events;
using Workarr.Tv;

namespace Sonarr.Api.V3.Episodes
{
    public abstract class EpisodeControllerWithSignalR : RestControllerWithSignalR<EpisodeResource, Episode>,
                                                         IHandle<EpisodeGrabbedEvent>,
                                                         IHandle<EpisodeImportedEvent>,
                                                         IHandle<EpisodeFileDeletedEvent>
    {
        protected readonly IEpisodeService _episodeService;
        protected readonly ISeriesService _seriesService;
        protected readonly IUpgradableSpecification _upgradableSpecification;
        protected readonly ICustomFormatCalculationService _formatCalculator;

        protected EpisodeControllerWithSignalR(IEpisodeService episodeService,
                                           ISeriesService seriesService,
                                           IUpgradableSpecification upgradableSpecification,
                                           ICustomFormatCalculationService formatCalculator,
                                           IBroadcastSignalRMessage signalRBroadcaster)
            : base(signalRBroadcaster)
        {
            _episodeService = episodeService;
            _seriesService = seriesService;
            _upgradableSpecification = upgradableSpecification;
            _formatCalculator = formatCalculator;
        }

        protected EpisodeControllerWithSignalR(IEpisodeService episodeService,
                                           ISeriesService seriesService,
                                           IUpgradableSpecification upgradableSpecification,
                                           ICustomFormatCalculationService formatCalculator,
                                           IBroadcastSignalRMessage signalRBroadcaster,
                                           string resource)
            : base(signalRBroadcaster)
        {
            _episodeService = episodeService;
            _seriesService = seriesService;
            _upgradableSpecification = upgradableSpecification;
            _formatCalculator = formatCalculator;
        }

        protected override EpisodeResource GetResourceById(int id)
        {
            var episode = _episodeService.GetEpisode(id);
            var resource = MapToResource(episode, true, true, true);
            return resource;
        }

        protected EpisodeResource MapToResource(Episode episode, bool includeSeries, bool includeEpisodeFile, bool includeImages)
        {
            var resource = episode.ToResource();

            if (includeSeries || includeEpisodeFile || includeImages)
            {
                var series = episode.Series ?? _seriesService.GetSeries(episode.SeriesId);

                if (includeSeries)
                {
                    resource.Series = series.ToResource();
                }

                if (includeEpisodeFile && episode.EpisodeFileId != 0)
                {
                    resource.EpisodeFile = episode.EpisodeFile.Value.ToResource(series, _upgradableSpecification, _formatCalculator);
                }

                if (includeImages)
                {
                    resource.Images = episode.Images;
                }
            }

            return resource;
        }

        protected List<EpisodeResource> MapToResource(List<Episode> episodes, bool includeSeries, bool includeEpisodeFile, bool includeImages)
        {
            var result = episodes.ToResource();

            if (includeSeries || includeEpisodeFile || includeImages)
            {
                var seriesDict = new Dictionary<int, Workarr.Tv.Series>();
                for (var i = 0; i < episodes.Count; i++)
                {
                    var episode = episodes[i];
                    var resource = result[i];

                    var series = episode.Series ?? seriesDict.GetValueOrDefault(episodes[i].SeriesId) ?? _seriesService.GetSeries(episodes[i].SeriesId);
                    seriesDict[series.Id] = series;

                    if (includeSeries)
                    {
                        resource.Series = series.ToResource();
                    }

                    if (includeEpisodeFile && episode.EpisodeFileId != 0)
                    {
                        resource.EpisodeFile = episode.EpisodeFile.Value.ToResource(series, _upgradableSpecification, _formatCalculator);
                    }

                    if (includeImages)
                    {
                        resource.Images = episode.Images;
                    }
                }
            }

            return result;
        }

        [NonAction]
        public void Handle(EpisodeGrabbedEvent message)
        {
            foreach (var episode in message.Episode.Episodes)
            {
                var resource = episode.ToResource();
                resource.Grabbed = true;

                BroadcastResourceChange(ModelAction.Updated, resource);
            }
        }

        [NonAction]
        public void Handle(EpisodeImportedEvent message)
        {
            foreach (var episode in message.EpisodeInfo.Episodes)
            {
                BroadcastResourceChange(ModelAction.Updated, episode.Id);
            }
        }

        [NonAction]
        public void Handle(EpisodeFileDeletedEvent message)
        {
            foreach (var episode in message.EpisodeFile.Episodes.Value)
            {
                BroadcastResourceChange(ModelAction.Updated, episode.Id);
            }
        }
    }
}
