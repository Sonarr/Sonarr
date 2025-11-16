using NzbDrone.Core.Languages;
using NzbDrone.Core.Qualities;

namespace Sonarr.Api.V5.Release;

public class ReleaseGrabResource
{
    public required string Guid { get; set; }
    public required int IndexerId { get; set; }
    public OverrideReleaseResource? Override { get; set; }
    public SearchInfoResource? SearchInfo { get; set; }
}

public class OverrideReleaseResource
{
    public int? SeriesId { get; set; }
    public List<int> EpisodeIds { get; set; } = [];
    public int? DownloadClientId { get; set; }
    public QualityModel? Quality { get; set; }
    public List<Language> Languages { get; set; } = [];
}

public class SearchInfoResource
{
    public int? SeriesId { get; set; }
    public int? SeasonNumber { get; set; }
    public int? EpisodeId { get; set; }
}
