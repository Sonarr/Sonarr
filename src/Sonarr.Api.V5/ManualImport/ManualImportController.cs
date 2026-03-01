using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles.EpisodeImport.Manual;
using NzbDrone.Core.Qualities;
using Sonarr.Http;
using Sonarr.Http.REST;

namespace Sonarr.Api.V5.ManualImport;

[V5ApiController]
public class ManualImportController : Controller
{
    private readonly IManualImportService _manualImportService;

    public ManualImportController(IManualImportService manualImportService)
    {
        _manualImportService = manualImportService;
    }

    [HttpGet]
    [Produces("application/json")]
    public List<ManualImportResource> GetMediaFiles(string? folder, [FromQuery] string[]? downloadIds, int? seriesId, int? seasonNumber, bool filterExistingFiles = true)
    {
        if (seriesId.HasValue && downloadIds == null)
        {
            return _manualImportService.GetMediaFiles(seriesId.Value, seasonNumber)
                .ToResource()
                .Select(AddQualityWeight)
                .ToList();
        }

        if (downloadIds != null && downloadIds.Any())
        {
            var files = new List<ManualImportItem>();

            foreach (var downloadId in downloadIds.Distinct())
            {
                files.AddRange(_manualImportService.GetMediaFiles(null, downloadId, seriesId, filterExistingFiles));
            }

            return files.ToResource()
                .Select(AddQualityWeight)
                .ToList();
        }

        return _manualImportService.GetMediaFiles(folder, null, seriesId, filterExistingFiles)
            .ToResource()
            .Select(AddQualityWeight)
            .ToList();
    }

    [HttpPost]
    [Consumes("application/json")]
    public List<ManualImportResource> ReprocessItems([FromBody] List<ManualImportReprocessResource> items)
    {
        if (items is { Count: 0 })
        {
            throw new BadRequestException("items must be provided");
        }

        var updatedItems = new List<ManualImportItem>();

        foreach (var item in items)
        {
            var processedItem = _manualImportService.ReprocessItem(item.Path, item.DownloadId, item.SeriesId, item.SeasonNumber, item.EpisodeIds ?? new List<int>(), item.ReleaseGroup, item.Quality, item.Languages, item.IndexerFlags, item.ReleaseType);

            // Only use the processed item's languages, quality, and release group if the user hasn't specified them.
            // Languages won't be returned when reprocessing if the season/episode isn't filled in yet and we don't want to return no languages to the client.
            if (processedItem.Languages.Empty() || item.Languages.Count > 1 || (item.Languages.SingleOrDefault() ?? Language.Unknown) == Language.Unknown)
            {
                processedItem.Languages = item.Languages;
            }

            if (item.Quality?.Quality != Quality.Unknown)
            {
                processedItem.Quality = item.Quality;
            }

            if (item.ReleaseGroup.IsNotNullOrWhiteSpace())
            {
                processedItem.ReleaseGroup = item.ReleaseGroup;
            }

            if (item.SeasonNumber.HasValue && !processedItem.SeasonNumber.HasValue)
            {
                processedItem.SeasonNumber = item.SeasonNumber;
            }

            updatedItems.Add(processedItem);
        }

        return updatedItems.ToResource();
    }

    private ManualImportResource AddQualityWeight(ManualImportResource item)
    {
        if (item.Quality != null)
        {
            item.QualityWeight = Quality.DefaultQualityDefinitions.Single(q => q.Quality == item.Quality.Quality).Weight;
            item.QualityWeight += item.Quality.Revision.Real * 10;
            item.QualityWeight += item.Quality.Revision.Version;
        }

        return item;
    }
}
