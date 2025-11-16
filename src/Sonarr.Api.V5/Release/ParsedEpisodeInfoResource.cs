using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace Sonarr.Api.V5.Release;

public class ParsedEpisodeInfoResource
{
    public QualityModel? Quality { get; set; }
    public string? ReleaseGroup { get; set; }
    public string? ReleaseHash { get; set; }
    public bool FullSeason { get; set; }
    public int SeasonNumber { get; set; }
    public string? AirDate { get; set; }
    public string? SeriesTitle { get; set; }
    public int[] EpisodeNumbers { get; set; } = [];
    public int[] AbsoluteEpisodeNumbers { get; set; } = [];
    public bool IsDaily { get; set; }
    public bool IsAbsoluteNumbering { get; set; }
    public bool IsPossibleSpecialEpisode { get; set; }
    public bool Special { get; set; }
}

public static class ParsedEpisodeInfoResourceMapper
{
    public static ParsedEpisodeInfoResource ToResource(this ParsedEpisodeInfo parsedEpisodeInfo)
    {
        return new ParsedEpisodeInfoResource
        {
            Quality = parsedEpisodeInfo.Quality,
            ReleaseGroup = parsedEpisodeInfo.ReleaseGroup,
            ReleaseHash = parsedEpisodeInfo.ReleaseHash,
            FullSeason = parsedEpisodeInfo.FullSeason,
            SeasonNumber = parsedEpisodeInfo.SeasonNumber,
            AirDate = parsedEpisodeInfo.AirDate,
            SeriesTitle = parsedEpisodeInfo.SeriesTitle,
            EpisodeNumbers = parsedEpisodeInfo.EpisodeNumbers,
            AbsoluteEpisodeNumbers = parsedEpisodeInfo.AbsoluteEpisodeNumbers,
            IsDaily = parsedEpisodeInfo.IsDaily,
            IsAbsoluteNumbering = parsedEpisodeInfo.IsAbsoluteNumbering,
            IsPossibleSpecialEpisode = parsedEpisodeInfo.IsPossibleSpecialEpisode,
            Special = parsedEpisodeInfo.Special,
        };
    }
}
