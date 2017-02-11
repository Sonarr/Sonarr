using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nancy;
using NLog;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;
using NzbDrone.SignalR;
using Sonarr.Api.V3.Series;
using Sonarr.Http;
using Sonarr.Http.Extensions;
using BadRequestException = Sonarr.Http.REST.BadRequestException;

namespace Sonarr.Api.V3.EpisodeFiles
{
    public class EpisodeFileModule : SonarrRestModuleWithSignalR<EpisodeFileResource, EpisodeFile>,
                                 IHandle<EpisodeFileAddedEvent>,
                                 IHandle<EpisodeFileDeletedEvent>
    {
        private readonly IMediaFileService _mediaFileService;
        private readonly IDeleteMediaFiles _mediaFileDeletionService;
        private readonly IRecycleBinProvider _recycleBinProvider;
        private readonly ISeriesService _seriesService;
        private readonly IQualityUpgradableSpecification _qualityUpgradableSpecification;

        public EpisodeFileModule(IBroadcastSignalRMessage signalRBroadcaster,
                             IMediaFileService mediaFileService,
                             IDeleteMediaFiles mediaFileDeletionService,
                             ISeriesService seriesService,
                             IQualityUpgradableSpecification qualityUpgradableSpecification,
            : base(signalRBroadcaster)
        {
            _mediaFileService = mediaFileService;
            _mediaFileDeletionService = mediaFileDeletionService;
            _seriesService = seriesService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;

            GetResourceById = GetEpisodeFile;
            GetResourceAll = GetEpisodeFiles;
            UpdateResource = SetQuality;
            DeleteResource = DeleteEpisodeFile;

            Put["/editor"] = episodeFiles => SetQuality();
            Delete["/bulk"] = episodeFiles => DeleteEpisodeFiles();
        }

        private EpisodeFileResource GetEpisodeFile(int id)
        {
            var episodeFile = _mediaFileService.Get(id);
            var series = _seriesService.GetSeries(episodeFile.SeriesId);

            return episodeFile.ToResource(series, _qualityUpgradableSpecification);
        }

        private List<EpisodeFileResource> GetEpisodeFiles()
        {
            var seriesIdQuery = Request.Query.SeriesId;
            var episodeFileIdsQuery = Request.Query.EpisodeFileIds;

            if (!seriesIdQuery.HasValue && !episodeFileIdsQuery.HasValue)
            {
                throw new BadRequestException("seriesId or episodeFileIds must be provided");
            }

            if (seriesIdQuery.HasValue)
            {
                int seriesId = Convert.ToInt32(seriesIdQuery.Value);
                var series = _seriesService.GetSeries(seriesId);

                return _mediaFileService.GetFilesBySeries(seriesId).ConvertAll(f => f.ToResource(series, _qualityUpgradableSpecification));
            }

            else
            {
                string episodeFileIdsValue = episodeFileIdsQuery.Value.ToString();

                var episodeFileIds = episodeFileIdsValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                        .Select(e => Convert.ToInt32(e))
                                                        .ToList();

                var episodeFiles = _mediaFileService.Get(episodeFileIds);

                return episodeFiles.GroupBy(e => e.SeriesId)
                                   .SelectMany(f => f.ToList()
                                                     .ConvertAll( e => e.ToResource(_seriesService.GetSeries(f.Key), _qualityUpgradableSpecification)))
                                   .ToList();
            }
        }

        private void SetQuality(EpisodeFileResource episodeFileResource)
        {
            var episodeFile = _mediaFileService.Get(episodeFileResource.Id);
            episodeFile.Quality = episodeFileResource.Quality;
            _mediaFileService.Update(episodeFile);
        }

        private Response SetQuality()
        {
            var resource = Request.Body.FromJson<EpisodeFileListResource>();
            var episodeFiles = _mediaFileService.GetFiles(resource.EpisodeFileIds);

            foreach (var episodeFile in episodeFiles)
            {
                episodeFile.Quality = resource.Quality;
            }

            _mediaFileService.Update(episodeFiles);

            var series = _seriesService.GetSeries(episodeFiles.First().SeriesId);

            return episodeFiles.ConvertAll(f => f.ToResource(series, _qualityUpgradableSpecification))
                               .AsResponse(HttpStatusCode.Accepted);
        }

        private void DeleteEpisodeFile(int id)
        {
            var episodeFile = _mediaFileService.Get(id);

            if (episodeFile == null)
            {
                throw new NzbDroneClientException(global::System.Net.HttpStatusCode.NotFound, "Episode file not found");
            }

            var series = _seriesService.GetSeries(episodeFile.SeriesId);

            _mediaFileDeletionService.DeleteEpisodeFile(series, episodeFile);
        }

        private Response DeleteEpisodeFiles()
        {
            var resource = Request.Body.FromJson<EpisodeFileListResource>();
            var episodeFiles = _mediaFileService.GetFiles(resource.EpisodeFileIds);
            var series = _seriesService.GetSeries(episodeFiles.First().SeriesId);

            foreach (var episodeFile in episodeFiles)
            {
                _mediaFileDeletionService.DeleteEpisodeFile(series, episodeFile);
            }

            return new object().AsResponse();
        }

        public void Handle(EpisodeFileAddedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.EpisodeFile.Id);
        }

        public void Handle(EpisodeFileDeletedEvent message)
        {
            BroadcastResourceChange(ModelAction.Deleted, message.EpisodeFile.Id);
        }
    }
}
