using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Commands;
using Sonarr.Http.Extensions;

namespace Sonarr.Api.V3.Series
{
    public class SeriesEditorModule : SonarrV3Module
    {
        private readonly ISeriesService _seriesService;
        private readonly IManageCommandQueue _commandQueueManager;

        public SeriesEditorModule(ISeriesService seriesService, IManageCommandQueue commandQueueManager)
            : base("/series/editor")
        {
            _seriesService = seriesService;
            _commandQueueManager = commandQueueManager;
            Put["/"] = series => SaveAll();
            Delete["/"] = series => DeleteSeries();
        }

        private Response SaveAll()
        {
            var resource = Request.Body.FromJson<SeriesEditorResource>();
            var seriesToUpdate = _seriesService.GetSeries(resource.SeriesIds);
            var seriesToMove = new List<BulkMoveSeries>();

            foreach (var series in seriesToUpdate)
            {
                if (resource.Monitored.HasValue)
                {
                    series.Monitored = resource.Monitored.Value;
                }

                if (resource.QualityProfileId.HasValue)
                {
                    series.ProfileId = resource.QualityProfileId.Value;
                }

                if (resource.LanguageProfileId.HasValue)
                {
                    series.LanguageProfileId = resource.LanguageProfileId.Value;
                }

                if (resource.SeriesType.HasValue)
                {
                    series.SeriesType = resource.SeriesType.Value;                    
                }

                if (resource.SeasonFolder.HasValue)
                {
                    series.SeasonFolder = resource.SeasonFolder.Value;
                }

                if (resource.RootFolderPath.IsNotNullOrWhiteSpace())
                {
                    series.RootFolderPath = resource.RootFolderPath;
                    seriesToMove.Add(new BulkMoveSeries
                                     {
                                         SeriesId = series.Id,
                                         SourcePath = series.Path
                                     });
                }

                if (resource.Tags != null)
                {
                    var newTags = resource.Tags;
                    var applyTags = resource.ApplyTags;

                    switch (applyTags)
                    {
                        case ApplyTags.Add:
                            newTags.ForEach(t => series.Tags.Add(t));
                            break;
                        case ApplyTags.Remove:
                            newTags.ForEach(t => series.Tags.Remove(t));
                            break;
                        case ApplyTags.Replace:
                            series.Tags = new HashSet<int>(newTags);
                            break;
                    }
                }
            }

            if (resource.MoveFiles && seriesToMove.Any())
            {
                _commandQueueManager.Push(new BulkMoveSeriesCommand
                                          {
                                              DestinationRootFolder = resource.RootFolderPath,
                                              Series = seriesToMove
                                          });
            }

            return _seriesService.UpdateSeries(seriesToUpdate, !resource.MoveFiles)
                                 .ToResource()
                                 .AsResponse(HttpStatusCode.Accepted);
        }

        private Response DeleteSeries()
        {
            var resource = Request.Body.FromJson<SeriesEditorResource>();

            foreach (var seriesId in resource.SeriesIds)
            {
                _seriesService.DeleteSeries(seriesId, false);
            }

            return new object().AsResponse();
        }
    }
}
