using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;

namespace Sonarr.Api.V5.Release;

public class ReleaseInfoResource
{
    public string? Guid { get; set; }
    public int Age { get; set; }
    public double AgeHours { get; set; }
    public double AgeMinutes { get; set; }
    public long Size { get; set; }
    public int IndexerId { get; set; }
    public string? Indexer { get; set; }
    public string? Title { get; set; }
    public int TvdbId { get; set; }
    public int TvRageId { get; set; }
    public string? ImdbId { get; set; }
    public IEnumerable<string> Rejections { get; set; } = [];
    public DateTime PublishDate { get; set; }
    public string? CommentUrl { get; set; }
    public string? DownloadUrl { get; set; }
    public string? InfoUrl { get; set; }
    public string? MagnetUrl { get; set; }
    public string? InfoHash { get; set; }
    public int? Seeders { get; set; }
    public int? Leechers { get; set; }
    public DownloadProtocol Protocol { get; set; }
    public int IndexerFlags { get; set; }
}

public static class ReleaseInfoResourceMapper
{
    public static ReleaseInfoResource ToResource(this ReleaseInfo releaseInfo)
    {
        var torrentInfo = releaseInfo as TorrentInfo ?? new TorrentInfo();
        var indexerFlags = torrentInfo.IndexerFlags;

        return new ReleaseInfoResource
        {
            Guid = releaseInfo.Guid,
            Age = releaseInfo.Age,
            AgeHours = releaseInfo.AgeHours,
            AgeMinutes = releaseInfo.AgeMinutes,
            Size = releaseInfo.Size,
            IndexerId = releaseInfo.IndexerId,
            Indexer = releaseInfo.Indexer,
            Title = releaseInfo.Title,
            TvdbId = releaseInfo.TvdbId,
            TvRageId = releaseInfo.TvRageId,
            ImdbId = releaseInfo.ImdbId,
            PublishDate = releaseInfo.PublishDate,
            CommentUrl = releaseInfo.CommentUrl,
            DownloadUrl = releaseInfo.DownloadUrl,
            InfoUrl = releaseInfo.InfoUrl,
            MagnetUrl = torrentInfo.MagnetUrl,
            InfoHash = torrentInfo.InfoHash,
            Seeders = torrentInfo.Seeders,
            Leechers = (torrentInfo.Peers.HasValue && torrentInfo.Seeders.HasValue) ? (torrentInfo.Peers.Value - torrentInfo.Seeders.Value) : (int?)null,
            Protocol = releaseInfo.DownloadProtocol,
            IndexerFlags = (int)indexerFlags,
        };
    }
}
