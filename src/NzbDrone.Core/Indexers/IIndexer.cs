using System;
using System.Collections.Generic;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers
{
    public interface IIndexer : IProvider
    {
        IParseFeed Parser { get; }
        DownloadProtocol Protocol { get; }
        Int32 SupportedPageSize { get; }
        Boolean SupportsPaging { get; }
        Boolean SupportsRss { get; }
        Boolean SupportsSearch { get; }

        IEnumerable<string> RecentFeed { get; }
        IEnumerable<string> GetEpisodeSearchUrls(List<String> titles, int tvRageId, int seasonNumber, int episodeNumber);
        IEnumerable<string> GetDailyEpisodeSearchUrls(List<String> titles, int tvRageId, DateTime date);
        IEnumerable<string> GetAnimeEpisodeSearchUrls(List<String> titles, int tvRageId, int absoluteEpisodeNumber);
        IEnumerable<string> GetSeasonSearchUrls(List<String> titles, int tvRageId, int seasonNumber, int offset);
        IEnumerable<string> GetSearchUrls(string query, int offset = 0);
    }
}