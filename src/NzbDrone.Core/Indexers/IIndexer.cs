using System;
using System.Collections.Generic;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers
{
    public interface IIndexer : IProvider
    {
        IParseFeed Parser { get; }
        DownloadProtocol Protocol { get; }
        Boolean SupportsPaging { get; }

        IEnumerable<string> RecentFeed { get; }
        IEnumerable<string> GetEpisodeSearchUrls(string seriesTitle, int tvRageId, int seasonNumber, int episodeNumber);
        IEnumerable<string> GetDailyEpisodeSearchUrls(string seriesTitle, int tvRageId, DateTime date);
        IEnumerable<string> GetSeasonSearchUrls(string seriesTitle, int tvRageId, int seasonNumber, int offset);
    }
}