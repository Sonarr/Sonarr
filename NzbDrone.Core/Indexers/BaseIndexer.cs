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

        IIndexerSetting Settings { get; }

        IEnumerable<string> GetEpisodeSearchUrls(string seriesTitle, int seasonNumber, int episodeNumber);
        IEnumerable<string> GetDailyEpisodeSearchUrls(string seriesTitle, DateTime date);
        IEnumerable<string> GetSeasonSearchUrls(string seriesTitle, int seasonNumber);
        IEnumerable<string> GetPartialSeasonSearchUrls(string seriesTitle, int seasonNumber, int episodeWildcard);
    }

    public abstract class BaseIndexer : IIndexerBase
    {
        public abstract string Name { get; }

        public virtual bool EnabledByDefault
        {
            get
            {
                return false;
            }
        }

        public virtual IParseFeed Parser
        {
            get
            {
                return new BasicRssParser();
            }
        }

        public virtual IIndexerSetting Settings
        {
            get
            {
                return new NullSetting();
            }
        }

        public abstract IEnumerable<string> RecentFeed { get; }

        public abstract IEnumerable<string> GetEpisodeSearchUrls(string seriesTitle, int seasonNumber, int episodeNumber);
        public abstract IEnumerable<string> GetDailyEpisodeSearchUrls(string seriesTitle, DateTime date);
        public abstract IEnumerable<string> GetSeasonSearchUrls(string seriesTitle, int seasonNumber);
        public abstract IEnumerable<string> GetPartialSeasonSearchUrls(string seriesTitle, int seasonNumber, int episodeWildcard);
    }


}