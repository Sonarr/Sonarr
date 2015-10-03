using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NzbDrone.Api.Extensions;
using NzbDrone.Api.Mapping;
using NzbDrone.Api.Series;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;
using NzbDrone.SignalR;

namespace NzbDrone.Api.Episodes
{
    public abstract class EpisodeModuleWithSignalR : NzbDroneRestModuleWithSignalR<EpisodeResource, Episode>,
        IHandle<EpisodeGrabbedEvent>,
        IHandle<EpisodeDownloadedEvent>
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
            episode.EpisodeFile.LazyLoad();
            episode.Series = _seriesService.GetSeries(episode.SeriesId);
            return ToResource(episode);
        }

        protected override EpisodeResource ToResource<TModel>(TModel model)
        {
            var resource = base.ToResource(model);

            var episode = model as Episode;
            if (episode != null)
            {
                if (episode.EpisodeFile.IsLoaded && episode.EpisodeFile.Value != null)
                {
                    resource.EpisodeFile.Path = Path.Combine(episode.Series.Path, episode.EpisodeFile.Value.RelativePath);
                    resource.EpisodeFile.QualityCutoffNotMet = _qualityUpgradableSpecification.CutoffNotMet(episode.Series.Profile.Value, episode.EpisodeFile.Value.Quality);
                }
            }

            return resource;
        }

        protected override List<EpisodeResource> ToListResource<TModel>(IEnumerable<TModel> modelList)
        {
            var resources =  base.ToListResource(modelList);

            return resources.LoadSubtype<EpisodeResource, SeriesResource, Core.Tv.Series>(e => e.SeriesId, _seriesService.GetSeries).ToList();
        }

        public void Handle(EpisodeGrabbedEvent message)
        {
            foreach (var episode in message.Episode.Episodes)
            {
                var resource = episode.InjectTo<EpisodeResource>();
                resource.Grabbed = true;

                BroadcastResourceChange(ModelAction.Updated, resource);
            }
        }

        public void Handle(EpisodeDownloadedEvent message)
        {
            foreach (var episode in message.Episode.Episodes)
            {
                BroadcastResourceChange(ModelAction.Updated, episode.Id);
            }
        }
    }
}