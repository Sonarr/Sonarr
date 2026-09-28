using System;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Parser;

public static class ReleaseComparer
{
    public static bool SameNzb(ReleaseComparerModel item, ReleaseInfo release)
    {
        if (!HasSameIndexer(item, release.Indexer))
        {
            return false;
        }

        return item.PublishedDate == release.PublishDate;
    }

    public static bool SameTorrent(ReleaseComparerModel item, TorrentInfo release)
    {
        if (release.InfoHash.IsNotNullOrWhiteSpace())
        {
            return release.InfoHash.Equals(item.TorrentInfoHash, StringComparison.InvariantCultureIgnoreCase);
        }

        return HasSameIndexer(item, release.Indexer);
    }

    private static bool HasSameIndexer(ReleaseComparerModel item, string indexer)
    {
        if (item.Indexer.IsNullOrWhiteSpace())
        {
            return true;
        }

        return item.Indexer.Equals(indexer, StringComparison.InvariantCultureIgnoreCase);
    }
}
