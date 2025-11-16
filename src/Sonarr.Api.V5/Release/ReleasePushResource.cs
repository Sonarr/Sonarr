using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;
using Sonarr.Http.REST;

namespace Sonarr.Api.V5.Release;

public class ReleasePushResource : RestResource
{
    public string? Guid { get; set; }
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
    public int? DownloadClientId { get; set; }
    public string? DownloadClientName { get; set; }
}

public static class ReleasePushResourceMapper
{
    public static ReleaseInfo ToModel(this ReleasePushResource resource)
    {
        ReleaseInfo model;

        if (resource.Protocol == DownloadProtocol.Torrent)
        {
            model = new TorrentInfo
            {
                MagnetUrl = resource.MagnetUrl,
                InfoHash = resource.InfoHash,
                Seeders = resource.Seeders,
                Peers = (resource.Seeders.HasValue && resource.Leechers.HasValue) ? (resource.Seeders + resource.Leechers) : null,
            };
        }
        else
        {
            model = new ReleaseInfo();
        }

        model.Guid = resource.Guid;
        model.Title = resource.Title;
        model.Size = resource.Size;
        model.DownloadUrl = resource.DownloadUrl;
        model.InfoUrl = resource.InfoUrl;
        model.CommentUrl = resource.CommentUrl;
        model.IndexerId = resource.IndexerId;
        model.Indexer = resource.Indexer;
        model.DownloadProtocol = resource.Protocol;
        model.TvdbId = resource.TvdbId;
        model.TvRageId = resource.TvRageId;
        model.ImdbId = resource.ImdbId;
        model.PublishDate = resource.PublishDate.ToUniversalTime();
        model.IndexerFlags = (IndexerFlags)resource.IndexerFlags;

        return model;
    }
}
