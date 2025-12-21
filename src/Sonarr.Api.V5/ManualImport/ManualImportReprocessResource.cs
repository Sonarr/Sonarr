using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using Sonarr.Api.V5.CustomFormats;
using Sonarr.Api.V5.Episodes;
using Sonarr.Http.REST;

namespace Sonarr.Api.V5.ManualImport;

public class ManualImportReprocessResource : RestResource
{
    public string? Path { get; set; }
    public int SeriesId { get; set; }
    public int? SeasonNumber { get; set; }
    public List<EpisodeResource> Episodes { get; set; } = [];
    public List<int>? EpisodeIds { get; set; }
    public QualityModel? Quality { get; set; }
    public List<Language> Languages { get; set; } = [];
    public string? ReleaseGroup { get; set; }
    public string? DownloadId { get; set; }
    public List<CustomFormatResource> CustomFormats { get; set; } = [];
    public int CustomFormatScore { get; set; }
    public int IndexerFlags { get; set; }
    public ReleaseType ReleaseType { get; set; }
    public IEnumerable<ImportRejectionResource> Rejections { get; set; } = [];
}
