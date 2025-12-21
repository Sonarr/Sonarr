using NzbDrone.Common.Crypto;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.MediaFiles.EpisodeImport.Manual;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using Sonarr.Api.V5.CustomFormats;
using Sonarr.Api.V5.Episodes;
using Sonarr.Api.V5.Series;
using Sonarr.Http.REST;

namespace Sonarr.Api.V5.ManualImport;

public class ManualImportResource : RestResource
{
    public string? Path { get; set; }
    public string? RelativePath { get; set; }
    public string? FolderName { get; set; }
    public string? Name { get; set; }
    public long Size { get; set; }
    public SeriesResource? Series { get; set; }
    public int? SeasonNumber { get; set; }
    public List<EpisodeResource> Episodes { get; set; } = [];
    public int? EpisodeFileId { get; set; }
    public string? ReleaseGroup { get; set; }
    public QualityModel? Quality { get; set; }
    public List<Language> Languages { get; set; } = [];
    public int QualityWeight { get; set; }
    public string? DownloadId { get; set; }
    public List<CustomFormatResource> CustomFormats { get; set; } = [];
    public int CustomFormatScore { get; set; }
    public int IndexerFlags { get; set; }
    public ReleaseType ReleaseType { get; set; }
    public IEnumerable<ImportRejectionResource> Rejections { get; set; } = [];
}

public static class ManualImportResourceMapper
{
    public static ManualImportResource ToResource(this ManualImportItem model)
    {
        var customFormats = model.CustomFormats;
        var customFormatScore = model.Series?.QualityProfile?.Value?.CalculateCustomFormatScore(customFormats) ?? 0;

        return new ManualImportResource
        {
            Id = HashConverter.GetHashInt31(model.Path),
            Path = model.Path,
            RelativePath = model.RelativePath,
            FolderName = model.FolderName,
            Name = model.Name,
            Size = model.Size,
            Series = model.Series?.ToResource(),
            SeasonNumber = model.SeasonNumber,
            Episodes = model.Episodes.ToResource(),
            EpisodeFileId = model.EpisodeFileId,
            ReleaseGroup = model.ReleaseGroup,
            Quality = model.Quality,
            Languages = model.Languages,
            CustomFormats = customFormats.ToResource(false),
            CustomFormatScore = customFormatScore,

            // QualityWeight
            DownloadId = model.DownloadId,
            IndexerFlags = model.IndexerFlags,
            ReleaseType = model.ReleaseType,
            Rejections = model.Rejections.Select(r => r.ToResource())
        };
    }

    public static List<ManualImportResource> ToResource(this IEnumerable<ManualImportItem> models)
    {
        return models.Select(ToResource).ToList();
    }
}

public class ImportRejectionResource
{
    public ImportRejectionReason Reason { get; set; }
    public string? Message { get; set; }
    public RejectionType Type { get; set; }
}

public static class ImportRejectionResourceMapper
{
    public static ImportRejectionResource ToResource(this ImportRejection rejection)
    {
        return new ImportRejectionResource
        {
            Reason = rejection.Reason,
            Message = rejection.Message,
            Type = rejection.Type
        };
    }
}
