using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Indexers
{
    public interface IIndexerBase
    {
        string Name { get; }
        bool EnabledByDefault { get; }

        IEnumerable<string> RecentFeed { get; }

        IParseFeed Parser { get; }

        bool IsConfigured { get; }

        IEnumerable<string> GetEpisodeSearchUrls(string seriesTitle, int seasonNumber, int episodeNumber);
        IEnumerable<string> GetDailyEpisodeSearchUrls(string seriesTitle, DateTime date);
        IEnumerable<string> GetSeasonSearchUrls(string seriesTitle, int seasonNumber);
        IEnumerable<string> GetPartialSeasonSearchUrls(string seriesTitle, int seasonNumber, int episodeWildcard);
    }
}