using System.Collections.Generic;
using Nancy;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tv;
using Sonarr.Http.Extensions;

namespace Sonarr.Api.V3.Series
{
    public class SeriesEditorModule : SonarrV3Module
    {
        private readonly ISeriesService _seriesService;

        public SeriesEditorModule(ISeriesService seriesService)
            : base("/series/editor")
        {
            _seriesService = seriesService;
            Put["/"] = series => SaveAll();
            Delete["/"] = series => DeleteSeries();
        }

        private Response SaveAll()
        {
            var resource = Request.Body.FromJson<SeriesEditorResource>();
            var seriesToUpdate = _seriesService.GetSeries(resource.SeriesIds);

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

            return _seriesService.UpdateSeries(seriesToUpdate)
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
