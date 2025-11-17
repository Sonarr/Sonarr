using System;
using System.Collections.Generic;
using NzbDrone.Core.Blocklisting;
using NzbDrone.Core.History;

namespace NzbDrone.Core.Parser.Model;

public class ReleaseComparerModel
{
    public string Title { get; set; }
    public string TorrentInfoHash { get; set; }
    public DateTime? PublishedDate { get; set; }
    public string Indexer { get; set; }
    public long Size { get; set; }

    public ReleaseComparerModel(Blocklist blocklist)
    {
        Title = blocklist.SourceTitle;
        TorrentInfoHash = blocklist.TorrentInfoHash;
        PublishedDate = blocklist.PublishedDate;
        Indexer = blocklist.Indexer;
        Size = blocklist.Size ?? 0;
    }

    public ReleaseComparerModel(EpisodeHistory history)
    {
        Title = history.SourceTitle;
        PublishedDate = history.Date;
        Indexer = history.Data.GetValueOrDefault("indexer");
        Size = long.Parse(history.Data.GetValueOrDefault("size", "0"));
    }
}
