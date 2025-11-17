using System;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Parser;

public static class ReleaseComparer
{
    public static bool SameNzb(ReleaseComparerModel item, ReleaseInfo release)
    {
        if (item.PublishedDate == release.PublishDate)
        {
            return true;
        }

        if (!HasSameIndexer(item, release.Indexer) &&
            HasSamePublishedDate(item, release.PublishDate) &&
            HasSameSize(item, release.Size))
        {
            return true;
        }

        return false;
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

    private static bool HasSamePublishedDate(ReleaseComparerModel item, DateTime publishedDate)
    {
        if (!item.PublishedDate.HasValue)
        {
            return true;
        }

        return item.PublishedDate.Value.AddMinutes(-2) <= publishedDate &&
               item.PublishedDate.Value.AddMinutes(2) >= publishedDate;
    }

    private static bool HasSameSize(ReleaseComparerModel item, long size)
    {
        if (item.Size == 0)
        {
            return true;
        }

        var difference = Math.Abs(item.Size - size);

        return difference <= 2.Megabytes();
    }
}
