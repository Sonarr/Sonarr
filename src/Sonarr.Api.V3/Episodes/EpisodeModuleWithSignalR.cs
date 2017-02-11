using System.Collections.Generic;
using System.IO;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;
using NzbDrone.SignalR;
using Sonarr.Api.V3.EpisodeFiles;
using Sonarr.Api.V3.Series;
using Sonarr.Http;
using Sonarr.Http.Extensions;
using Sonarr.Http.Mapping;

namespace Sonarr.Api.V3.Episodes
{
    public abstract class EpisodeModuleWithSignalR : SonarrRestModuleWithSignalR<EpisodeResource, Episode>,
        IHandle<EpisodeGrabbedEvent>,
        IHandle<EpisodeImportedEvent>
    {
        protected readonly IEpisodeService _episodeService;
        protected readonly ISeriesService _seriesService;
        protected readonly IQualityUpgradableSpecification _qualityUpgradableSpecification;

        protected EpisodeModuleWithSignalR(IEpisodeService episodeService,
                                           ISeriesService seriesService,
                                           IQualityUpgradableSpecification qualityUpgradableSpecification,
                                           IBroadcastSignalRMessage signalRBroadcaster)
            : base(signalRBroadcaster)
        {
            _episodeService = episodeService;
            _seriesService = seriesService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;

            GetResourceById = GetEpisode;
        }

        protected EpisodeModuleWithSignalR(IEpisodeService episodeService,
                                           ISeriesService seriesService,
                                           IQualityUpgradableSpecification qualityUpgradableSpecification,
                                           IBroadcastSignalRMessage signalRBroadcaster,
                                           string resource)
            : base(signalRBroadcaster, resource)
        {
            _episodeService = episodeService;
            _seriesService = seriesService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;

            GetResourceById = GetEpisode;
        }

        protected EpisodeResource GetEpisode(int id)
        {
            var episode = _episodeService.GetEpisode(id);
            var resource = MapToResource(episode, true, true);
            return resource;
        }

        protected EpisodeResource MapToResource(Episode episode, bool includeSeries, bool includeEpisodeFile)
        {
            var resource = episode.ToResource();

            if (includeSeries || includeEpisodeFile)
            {
                var series = episode.Series ?? _seriesService.GetSeries(episode.SeriesId);

                if (includeSeries)
                {
                    resource.Series = series.ToResource();
                }
                if (includeEpisodeFile && episode.EpisodeFileId != 0)
                {
                    resource.EpisodeFile = episode.EpisodeFile.Value.ToResource(series, _qualityUpgradableSpecification);
                }
            }

            return resource;
        }

        protected List<EpisodeResource> MapToResource(List<Episode> episodes, bool includeSeries, bool includeEpisodeFile)
        {
            var result = episodes.ToResource();

            if (includeSeries || includeEpisodeFile)
            {
                var seriesDict = new Dictionary<int, NzbDrone.Core.Tv.Series>();
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
                    if (includeEpisodeFile && episodes[i].EpisodeFileId != 0)
                    {
                        resource.EpisodeFile = episodes[i].EpisodeFile.Value.ToResource(series, _qualityUpgradableSpecification);
                    }
                }
            }

            return result;
        }

        public void Handle(EpisodeGrabbedEvent message)
        {
            foreach (var episode in message.Episode.Episodes)
            {
                var resource = episode.ToResource();
                resource.Grabbed = true;

                BroadcastResourceChange(ModelAction.Updated, resource);
            }
        }

        public void Handle(EpisodeImportedEvent message)
        {
            if (!message.NewDownload)
            {
                return;
            }

            foreach (var episode in message.EpisodeInfo.Episodes)
            {
                BroadcastResourceChange(ModelAction.Updated, episode.Id);
            }
        }
    }
}
