using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Tv;
using Sonarr.Api.V5.CustomFormats;
using Sonarr.Api.V5.Series;
using Sonarr.Http.REST;

namespace Sonarr.Api.V5.Release;

public class ReleaseResource : RestResource
{
    public ParsedEpisodeInfoResource? ParsedInfo { get; set; }
    public ReleaseInfoResource? Release { get; set; }
    public ReleaseDecisionResource? Decision { get; set; }
    public int QualityWeight { get; set; }
    public List<Language> Languages { get; set; } = [];
    public int? MappedSeasonNumber { get; set; }
    public int[] MappedEpisodeNumbers { get; set; } = [];
    public int[] MappedAbsoluteEpisodeNumbers { get; set; } = [];
    public int? MappedSeriesId { get; set; }
    public IEnumerable<ReleaseEpisodeResource> MappedEpisodeInfo { get; set; } = [];
    public bool EpisodeRequested { get; set; }
    public bool DownloadAllowed { get; set; }
    public int ReleaseWeight { get; set; }
    public List<CustomFormatResource>? CustomFormats { get; set; }
    public int CustomFormatScore { get; set; }
    public AlternateTitleResource? SceneMapping { get; set; }
}

public static class ReleaseResourceMapper
{
    public static ReleaseResource ToResource(this DownloadDecision model)
    {
        var releaseInfo = model.RemoteEpisode.Release;
        var parsedEpisodeInfo = model.RemoteEpisode.ParsedEpisodeInfo;
        var remoteEpisode = model.RemoteEpisode;

        return new ReleaseResource
        {
            ParsedInfo = parsedEpisodeInfo.ToResource(),
            Release = releaseInfo.ToResource(),
            Decision = new ReleaseDecisionResource(model),

            Languages = remoteEpisode.Languages,
            MappedSeriesId = remoteEpisode.Series?.Id,
            MappedSeasonNumber = remoteEpisode.Episodes.FirstOrDefault()?.SeasonNumber,
            MappedEpisodeNumbers = remoteEpisode.Episodes.Select(v => v.EpisodeNumber).ToArray(),
            MappedAbsoluteEpisodeNumbers = remoteEpisode.Episodes.Where(v => v.AbsoluteEpisodeNumber.HasValue).Select(v => v.AbsoluteEpisodeNumber!.Value).ToArray(),
            MappedEpisodeInfo = remoteEpisode.Episodes.Select(v => new ReleaseEpisodeResource(v)),
            EpisodeRequested = remoteEpisode.EpisodeRequested,
            DownloadAllowed = remoteEpisode.DownloadAllowed,
            CustomFormatScore = remoteEpisode.CustomFormatScore,
            CustomFormats = remoteEpisode.CustomFormats?.ToResource(false),
            SceneMapping = remoteEpisode.SceneMapping?.ToResource(),
        };
    }

    public static List<ReleaseResource> MapDecisions(this IEnumerable<DownloadDecision> decisions, QualityProfile profile)
    {
        var result = new List<ReleaseResource>();

        foreach (var downloadDecision in decisions)
        {
            var release = MapDecision(downloadDecision, result.Count, profile);

            result.Add(release);
        }

        return result;
    }

    public static ReleaseResource MapDecision(this DownloadDecision decision, int initialWeight, QualityProfile profile)
    {
        var release = decision.ToResource();

        release.ReleaseWeight = initialWeight;

        if (release.ParsedInfo?.Quality == null)
        {
            release.QualityWeight = 0;
        }
        else
        {
            release.QualityWeight = profile.GetIndex(release.ParsedInfo.Quality.Quality).Index * 100;
            release.QualityWeight += release.ParsedInfo.Quality.Revision.Real * 10;
            release.QualityWeight += release.ParsedInfo.Quality.Revision.Version;
        }

        return release;
    }
}

public class ReleaseEpisodeResource
{
    public int Id { get; set; }
    public int SeasonNumber { get; set; }
    public int EpisodeNumber { get; set; }
    public int? AbsoluteEpisodeNumber { get; set; }
    public string? Title { get; set; }

    public ReleaseEpisodeResource()
    {
    }

    public ReleaseEpisodeResource(Episode episode)
    {
        Id = episode.Id;
        SeasonNumber = episode.SeasonNumber;
        EpisodeNumber = episode.EpisodeNumber;
        AbsoluteEpisodeNumber = episode.AbsoluteEpisodeNumber;
        Title = episode.Title;
    }
}
