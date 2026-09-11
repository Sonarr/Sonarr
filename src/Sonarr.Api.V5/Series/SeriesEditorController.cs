using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tv;
using Sonarr.Http;

namespace Sonarr.Api.V5.Series;

[V5ApiController("series/editor")]
public class SeriesEditorController : Controller
{
    private readonly ISeriesService _seriesService;
    private readonly SeriesEditorValidator _seriesEditorValidator;

    public SeriesEditorController(ISeriesService seriesService, SeriesEditorValidator seriesEditorValidator)
    {
        _seriesService = seriesService;
        _seriesEditorValidator = seriesEditorValidator;
    }

    [HttpPut]
    public Results<Ok<List<SeriesResource>>, BadRequest> SaveAll([FromBody] SeriesEditorResource resource)
    {
        var seriesToUpdate = _seriesService.GetSeries(resource.SeriesIds);

        foreach (var series in seriesToUpdate)
        {
            if (resource.Monitored.HasValue)
            {
                series.Monitored = resource.Monitored.Value;
            }

            if (resource.MonitorNewItems.HasValue)
            {
                series.MonitorNewItems = resource.MonitorNewItems.Value;
            }

            if (resource.QualityProfileId.HasValue)
            {
                series.QualityProfileId = resource.QualityProfileId.Value;
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

            var validationResult = _seriesEditorValidator.Validate(series);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
        }

        var updated = _seriesService.UpdateSeries(seriesToUpdate, resource.MoveFiles);

        return TypedResults.Ok(updated.ToResource());
    }

    [HttpDelete]
    public NoContent DeleteSeries([FromBody] SeriesEditorResource resource)
    {
        _seriesService.DeleteSeries(resource.SeriesIds, resource.DeleteFiles, resource.AddImportListExclusion);

        return TypedResults.NoContent();
    }
}
